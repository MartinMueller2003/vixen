using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vixen.Sys;
using VixenModules.Sequence.Timed;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using Vixen.Services;
using Vixen.Module.Effect;
using VixenModules.App.Curves;
using VixenModules.App.ColorGradients;
using System.Drawing;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	public class Vixen3SequenceCreator
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ISequence Sequence { get; set; }

		private CoversionProgressForm conversionProgressBar = null;
		public LightOramaSequenceData parsedLightOramaSequence = null;
		private List<LorChannelMapping> mappings = null;

		/// <summary>
		/// Convert LOR Sequence data into a Vixen 3 sequence
		/// </summary>
		/// <param name="LightOramaSequence"></param>
		/// <param name="list"></param>
		public Vixen3SequenceCreator(LightOramaSequenceData LightOramaSequence, List<LorChannelMapping> list)
		{
			parsedLightOramaSequence = LightOramaSequence;
			mappings = list;

			conversionProgressBar = new CoversionProgressForm();
			conversionProgressBar.Show();

			conversionProgressBar.SetupProgressBar(0, parsedLightOramaSequence.mappings.Count);

			conversionProgressBar.StatusLineLabel = "Converting Light-O-Rama sequence";

			createTimedSequence();
			importSequenceData();

			conversionProgressBar.Close();
		} // Vixen3SequenceCreator

		private void createTimedSequence()
		{
			Sequence = new TimedSequence() { SequenceData = new TimedSequenceData() };

			// TODO: use this mark collection (maybe generate a grid?)
			//I am not sure what to do with this, but it looks like John had a plan.
			MarkCollection mc = new MarkCollection();

			Sequence.Length = TimeSpan.FromMilliseconds(parsedLightOramaSequence.SeqLengthInMills);

			var songFileName = parsedLightOramaSequence.SongPath + Path.DirectorySeparatorChar + parsedLightOramaSequence.SongFileName;
			if (songFileName != null)
			{
				if (File.Exists(songFileName))
				{
					Sequence.AddMedia(MediaService.Instance.GetMedia(songFileName));
				}
				else
				{
					var message = string.Format("Could not locate the audio file '{0}'; please add it manually " +
												"after import (Under Tools -> Associate Audio).", Path.GetFileName(songFileName));
					MessageBox.Show(message, "Couldn't find audio");
				}
			}
		} // createTimedSequence

		/// <summary>
		/// Convert parsedLightOramaSequence into a V3 sequence
		/// </summary>
		private void importSequenceData()
		{
			// instantiate the state machine to process incoming data
			// LightOramaSequenceImportSM import = new LightOramaSequenceImportSM(Sequence, parsedLightOramaSequence.EventPeriod);

			// the current color is based on the intensity of a three channel group
			int red = 0;
			int green = 0;
			int blue = 0;

			// for each channel in the LightOrama sequence
			foreach (LorChannelMapping channelMapping in mappings)
			{
				conversionProgressBar.IncrementProgressBar();
				Application.DoEvents();

				// is this channel defined in the LOR channel list?
				if( false == parsedLightOramaSequence.SequenceObjects.ContainsKey(Convert.ToUInt64(channelMapping.ChannelNumber)) )
				{
					// channel is not in our list
					Logging.Error("Channel " + channelMapping.ChannelNumber + " in the mapping table does not exist in the LOR channel table.");
					continue;
				} // end not a valid channel

				// is this an unmapped output channel?
				if (Guid.Empty == channelMapping.ElementNodeId)
				{
					// no output channel. Move on to the next channel
					continue;
				} // end no output channel defined

				LorChannel lorChannel = parsedLightOramaSequence.SequenceObjects[Convert.ToUInt64(channelMapping.ChannelNumber)] as LorChannel;
				ElementNode vixElement = VixenSystem.Nodes.GetElementNode(channelMapping.ElementNodeId);
				if (null == vixElement)
				{
					Logging.Error("Vixen Element " + channelMapping.ElementNodeId + " could not be located.");
					continue;
				}

				// translate each effect associated with this channel
				foreach (ILorEffect effect in lorChannel.Effects)
				{
					// Get the output color
					Color color = (Color.Empty == channelMapping.DestinationColor) ? lorChannel.Color : channelMapping.DestinationColor;

					// translate the effect
					EffectNode node = effect.translateEffect(vixElement, color);
					if( null != node)
					{
						Sequence.InsertData(node);
					}
				} // end translate the effect

			} // end process each channel
#if foo
			for (var currentElementNum = 0; currentElementNum < parsedLightOramaSequence.LorChannels.Count; currentElementNum++)
			{
				// Check to see if we are processing more elements than we have mappings. This showed up as an error for a user
				if (currentElementNum >= mappings.Count)
				{
					Logging.Error("importSequenceData: Trying to process more elements (" + parsedLightOramaSequence.LorChannels.Count + ") than we have mappings. (" + mappings.Count + ")");
					break;
				}
				ChannelMapping LightOramaChannelMapping = mappings[currentElementNum];

				// set the channel number and the time for each LightOrama event.
				// import.OpenChannel(LightOramaChannelMapping.ElementNodeId, Convert.ToDouble(parsedLightOramaSequence.EventPeriod));

				// Logging.Debug("importSequenceData:currentElementNum: " + currentElementNum);

				string elementName = LightOramaChannelMapping.ChannelName;
				Color currentColor = Color.White;
				byte currentIntensity = 0;

				// do we have a valid guid conversion?
				if (false == m_GuidToLightOramaChanList.ContainsKey(LightOramaChannelMapping.ElementNodeId))
				{
					Logging.Error("importSequenceData: Configuration error. GUID: '" + LightOramaChannelMapping.ElementNodeId + "' not found in m_GuidToLightOramaChanList.");
					continue;
				}

				// is this a valid pixel configuration
				if ((true == LightOramaChannelMapping.ColorMixing) && (3 != m_GuidToLightOramaChanList[LightOramaChannelMapping.ElementNodeId].Count))
				{
					Logging.Error("importSequenceData: Configuration error. Found '" + m_GuidToLightOramaChanList[LightOramaChannelMapping.ElementNodeId].Count + "' Light-O-Rama channels attached to element '" + elementName + "'. Expected 3(RGB). Converting element to non color mixing mode.");
					LightOramaChannelMapping.ColorMixing = false;
				}

				// process each event for this LightOrama channel
				for (uint currentEventNum = 0; currentEventNum < parsedLightOramaSequence.EventsPerElement; currentEventNum++)
				{
					// get the intensity for this LightOrama channel
					currentIntensity = parsedLightOramaSequence.EventData[currentElementNum * parsedLightOramaSequence.EventsPerElement + currentEventNum];

					// is this an RGB Pixel?
					if (true == LightOramaChannelMapping.ColorMixing)
					{
						// Only process the RED channel of a three channel pixel
						if (Color.Red != LightOramaChannelMapping.DestinationColor)
						{
							// this is not the red channel of a pixel
							continue;
						} // end not red pixel channel

						red = 0;
						green = 0;
						blue = 0;

						// process the input colors bound to this output channel
						foreach (ChannelMapping LightOramaChannel in m_GuidToLightOramaChanList[LightOramaChannelMapping.ElementNodeId])
						{
							// Logging.Info("convertMapping: Processing LightOrama Channel '" + LightOramaChannel.ChannelName + "' color intensity.");

							switch (LightOramaChannel.DestinationColor.Name)
							{
								case "Red":
									{
										red = Math.Max(red, parsedLightOramaSequence.EventData[(Convert.ToInt64(LightOramaChannel.ChannelNumber) - 1) * parsedLightOramaSequence.EventsPerElement + currentEventNum]);
										break;
									} // end Red

								case "Green":
									{
										green = Math.Max(green, parsedLightOramaSequence.EventData[(Convert.ToInt64(LightOramaChannel.ChannelNumber) - 1) * parsedLightOramaSequence.EventsPerElement + currentEventNum]);
										break;
									} // end Green

								case "Blue":
									{
										blue = Math.Max(blue, parsedLightOramaSequence.EventData[(Convert.ToInt64(LightOramaChannel.ChannelNumber) - 1) * parsedLightOramaSequence.EventsPerElement + currentEventNum]);
										break;
									} // end Red

								default:
									{
										Logging.Error("importSequenceData pixel conversion processing error. Skipping processing unexpected color '" + LightOramaChannel.DestinationColor.Name + "' for LightOrama Channel '" + LightOramaChannel.ChannelName + "(" + LightOramaChannel.ChannelNumber + ")'. Color must be one of 'RED', 'GREEN' or 'BLUE'");
										break;
									} // end default
							} // end switch on color
						} // end process each LightOrama channel assigned to the v3 channel

						// get the max intensity for this LightOrama channel set
						currentIntensity = Convert.ToByte(Math.Min((int)255, Math.Max(red, Math.Max(green, blue))));

						// Scale the color to full intensity and let the intensity value attenuate it.
						if (0 != currentIntensity)
						{
							double multplier = Convert.ToDouble(byte.MaxValue) / Convert.ToDouble(currentIntensity);

							red = Math.Min(((int)255), Convert.ToInt32(Convert.ToDouble(red) * multplier));
							green = Math.Min(((int)255), Convert.ToInt32(Convert.ToDouble(green) * multplier));
							blue = Math.Min(((int)255), Convert.ToInt32(Convert.ToDouble(blue) * multplier));
						}

						// set the final color
						currentColor = Color.FromArgb(red, green, blue);
					} // end pixel processing
					else
					{
						// set the non pixel color value
						currentColor = mappings[currentElementNum].DestinationColor;
					} // end non pixel processing

					// process the event through the state machine.
					// import.processEvent(currentEventNum, currentColor, currentIntensity);
				} // end for each event in the element / channel

				// close this channel
				// import.closeChannel();
			} // end for each input channel
#endif // foo
		} // end importSequenceData
	}
}