using System;
using System.Collections.Generic;
using System.Linq;
using Vixen.Sys;
using System.Drawing;
using System.Xml.Serialization;

namespace VixenModules.SequenceType.LightOrama
{
	[Serializable]
	public class LorChannelMapping
	{
		public string ChannelName = string.Empty;
		public Color ChannelColor = new Color();
		public UInt64 ChannelNumber = 0;
		public Color DestinationColor = new Color();
		public Guid ElementNodeId = Guid.Empty;
		public bool ColorMixing = false;

		public LorChannelMapping( string channelName, 
								  Color channelColor, 
								  UInt64 channelNumber,
								  Guid nodeId,
								  Color destinationColor,
								  bool colorMixing)
		{
			ChannelName = channelName;
			ChannelColor = channelColor;
			ChannelNumber = channelNumber;
			ElementNodeId = nodeId;
			DestinationColor = destinationColor;
			ColorMixing = colorMixing;
		} // LorChannelMapping
	} // LorChannelMapping
} // VixenModules.SequenceType.LightOrama