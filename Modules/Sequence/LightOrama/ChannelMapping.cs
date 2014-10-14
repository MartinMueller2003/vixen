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
		public string ChannelOutput = string.Empty;
		public UInt64 ChannelNumber = 0;
		public Color DestinationColor = new Color();
		public Guid ElementNodeId = Guid.Empty;
		public bool ColorMixing = false;


		public LorChannelMapping(string channelName, Color channelColor, UInt64 channelNumber, string channelOutput, Guid nodeId,
							  Color destinationColor, bool colorMixing)
		{
			ChannelName = channelName;
			ChannelColor = channelColor;
			ChannelOutput = channelOutput;
			ChannelNumber = channelNumber;
			ElementNodeId = nodeId;
			DestinationColor = destinationColor;
			ColorMixing = colorMixing;
		}

		public LorChannelMapping(string channelName, Color channelColor, UInt64 channelNumber, string channelOutput, Guid nodeId,
		                      Color destinationColor)
		{
			ChannelName = channelName;
			ChannelColor = channelColor;
			ChannelOutput = channelOutput;
			ChannelNumber = channelNumber;
			ElementNodeId = nodeId;
			DestinationColor = destinationColor;
		}

		public LorChannelMapping(string channelName, Color channelColor, UInt64 channelNumber, string channelOutput)
		{
			ChannelName = channelName;
			ChannelColor = channelColor;
			ChannelOutput = channelOutput;
			ChannelNumber = channelNumber;
		}

		public LorChannelMapping()
		{
		}
	}
}