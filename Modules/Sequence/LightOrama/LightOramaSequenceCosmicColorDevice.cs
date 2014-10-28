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
	public class LorCosmicColorDevice : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }

		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorCosmicColorDevice(XElement element, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			Children = new List<UInt64>();
			Parents = new List<UInt64>();
			Name = string.Empty;
			Index = 0;
			TotalMs = 0;
			m_sequenceObjects = sequenceObjects;
			Parse(element);
		} // LorCosmicColorDevice

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
						Logging.Error("Skipping unsupported LOR sequence Element ...cosmicColorDevice.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // LorCosmicColorDevice
} // VixenModules.SequenceType.LightOrama