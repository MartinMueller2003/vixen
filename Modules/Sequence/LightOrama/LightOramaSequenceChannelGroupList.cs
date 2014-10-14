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

		public string Name { get; private set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public bool HasParent { get; set; }

		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorChannelGroupList(XElement element, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			Children = new List<UInt64>();
			Name = string.Empty;
			Index = 0;
			TotalMs = 0;
			HasParent = false;
			m_sequenceObjects = sequenceObjects;
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
							UInt64 index = (null == channelGroup.Attribute("savedIndex")) ? UInt64.MaxValue : UInt64.Parse(channelGroup.Attribute("savedIndex").Value);
							Children.Add(index);
							// mark the child as having a parent
							m_sequenceObjects[index].HasParent = true;
						}
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...channelGroupList.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // LorChannelGroupList
} // VixenModules.SequenceType.LightOrama