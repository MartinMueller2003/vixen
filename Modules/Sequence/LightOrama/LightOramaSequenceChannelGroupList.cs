using NLog;
using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Vixen.Module.Effect;
using Vixen.Services;
using Vixen.Sys;
using VixenModules.App.Curves;
using VixenModules.App.ColorGradients;
using VixenModules.Sequence.Timed;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	public class LorChannelGroupList : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public Guid ElementId { get; private set; }

		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorChannelGroupList(XElement element, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			Children = new List<UInt64>();
			Parents = new List<UInt64>();
			Name = string.Empty;
			Index = 0;
			TotalMs = 0;
			m_sequenceObjects = sequenceObjects;
			ElementId = Guid.Empty;
			Parse(element);
		} // LorChannelGroupList

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="channel"></param>
		public void Parse(XElement channel)
		{
			Index = (channel.Attribute("savedIndex") == null) ? Index : UInt64.Parse(channel.Attribute("savedIndex").Value);
			TotalMs = (channel.Attribute("totalCentiseconds") == null) ? Index : (UInt64.Parse(channel.Attribute("totalCentiseconds").Value) * 10);
			Name = (channel.Attribute("name") == null) ? Name : channel.Attribute("name").Value;

			foreach (XElement element in channel.Elements().ToList())
			{
				switch (element.Name.ToString())
				{
					case "channelGroups":
						foreach (XElement channelGroup in element.Elements("channelGroup").ToList())
						{
							UInt64 childIndex = (null == channelGroup.Attribute("savedIndex")) ? UInt64.MaxValue : UInt64.Parse(channelGroup.Attribute("savedIndex").Value);

							// do we already know about this child (reparse)?
							if (0 == Children.Where(x => x == childIndex).ToList().Count)
							{
								Children.Add(childIndex);
							} // end link parent to child

							// do the child already know about this parent (reparse)?
							if (0 == m_sequenceObjects[childIndex].Parents.Where(x => x == Index).ToList().Count)
							{
								// mark the child as having a parent
								m_sequenceObjects[childIndex].Parents.Add(Index);
							} // end link child to parent
						}
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...channelGroupList.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse

		/// <summary>
		/// create this element in a tree of elements. Create any parents as needed
		/// </summary>
		/// <param name="sequence"></param>
		public void CreateVixenElement(LightOramaSequenceData sequence)
		{
			do
			{
				// do we already have an element ID
				if (Guid.Empty != ElementId)
				{
					// just go away
					break;
				} // end filter creation of element

				// create an element for this lor object
				ElementNode element = ElementNodeService.Instance.CreateSingle(null, Name);
				ElementId = element.Id;

				// Bind to the parent nodes.
				foreach (UInt64 parentId in Parents)
				{
					ILorObject parentObject = sequence.SequenceObjects[parentId];
					if (Guid.Empty == parentObject.ElementId)
					{
						parentObject.CreateVixenElement(sequence);
					} // end create parent object

					// bind the parent to this node
					ElementNode parentElement = VixenSystem.Nodes.GetElementNode(parentObject.ElementId);
					VixenSystem.Nodes.AddChildToParent(element, parentElement);
				} // end bind to parents

				// check to see if we should be at the root level?
				if (0 != Parents.Count)
				{
					VixenSystem.Nodes.RemoveNode(element, null, true);
				}

			} while (false);
		} // CreateVixenElement

		/// <summary>
		/// Update the mappings for this channel
		/// </summary>
		/// <param name="dataSet"></param>
		/// <returns></returns>
		public int AddToMappings(LightOramaSequenceData sequence)
		{
			return 0;
		} // AddToMappings
	} // LorChannelGroupList
} // VixenModules.SequenceType.LightOrama