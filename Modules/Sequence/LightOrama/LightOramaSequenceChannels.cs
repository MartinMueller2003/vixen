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
	public class LorChannels
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		private CoversionProgressForm ImportProgressBar = null;
		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorChannels(Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			m_sequenceObjects = sequenceObjects;
		} // LorChannels

		/// <summary>
		/// Build the hierarchy that exists below the Tracks node in the LOR sequence
		/// </summary>
		/// <param name="channels"></param>
		public void Parse(XElement channels)
		{
			// set up the progress bar
			ImportProgressBar = new CoversionProgressForm();
			ImportProgressBar.Show();
			ImportProgressBar.SetupProgressBar(0, channels.Elements().ToList().Count);
			ImportProgressBar.StatusLineLabel = "Importing Light-O-Rama sequence";

			// process each element
			foreach (XElement element in channels.Elements().ToList())
			{
				ImportProgressBar.IncrementProgressBar();
				Application.DoEvents();

				// Logging.Info("Element Name ...channels.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "channel":
						// set the index of the channel
						UInt64 index = (element.Attribute("savedIndex") == null) ? 0 : UInt64.Parse(element.Attribute("savedIndex").Value);

						// do we already have this channel
						if (false == m_sequenceObjects.ContainsKey(index))
						{
							LorChannel newChannel = new LorChannel(element);
							// need to add the channel
							m_sequenceObjects.Add(index, newChannel);
						} // end create channel
						else
						{
							// update the channel data
							m_sequenceObjects[index].Parse(element);
						}
						break;

					case "rgbChannel":
						// set the index of the channel
						index = (element.Attribute("savedIndex") == null) ? 0 : UInt64.Parse(element.Attribute("savedIndex").Value);

						// do we already have this channel
						if (false == m_sequenceObjects.ContainsKey(index))
						{
							LorRgbChannel newChannel = new LorRgbChannel(element, m_sequenceObjects);
							// need to add the channel
							m_sequenceObjects.Add(index, newChannel);
						} // end create channel
						else
						{
							m_sequenceObjects[index].Parse(element);
						}
						break;

					case "cosmicColorDevice":
						// set the index of the channel
						index = (element.Attribute("savedIndex") == null) ? 0 : UInt64.Parse(element.Attribute("savedIndex").Value);

						// do we already have this channel
						if (false == m_sequenceObjects.ContainsKey(index))
						{
							LorCosmicColorDevice newChannel = new LorCosmicColorDevice(element, m_sequenceObjects);
							// need to add the channel
							m_sequenceObjects.Add(index, newChannel);
						} // end create channel
						else
						{
							m_sequenceObjects[index].Parse(element);
						}
						break;

					case "channelGroupList":
						// set the index of the channel
						index = (element.Attribute("savedIndex") == null) ? 0 : UInt64.Parse(element.Attribute("savedIndex").Value);

						// do we already have this channel
						if (false == m_sequenceObjects.ContainsKey(index))
						{
							LorChannelGroupList newChannel = new LorChannelGroupList(element, m_sequenceObjects);
							// need to add the channel
							m_sequenceObjects.Add(index, newChannel);
						} // end create channel
						else
						{
							m_sequenceObjects[index].Parse(element);
						}
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...channels.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level

			// post process the RGB channels
			foreach (LorRgbChannel rgbChannel in m_sequenceObjects.Values.OfType<LorRgbChannel>().ToList())
			{
				// process each channel that is a member of this rgb group
				foreach( UInt64 index in rgbChannel.Children )
				{
					// link the channel to the RGB channel group
					(m_sequenceObjects[index] as LorChannel).RgbChannelId = rgbChannel.Index;
				} // end process member channels
			} // end post process the rgb channels

			ImportProgressBar.Close();
		} // Parse
	} // class LorChannels
} // VixenModules.SequenceType.LightOrama