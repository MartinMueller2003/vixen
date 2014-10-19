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

		private CoversionProgressForm m_conversionProgressBar = null;
		private LightOramaSequenceData m_parsedLightOramaSequence = null;
		private List<LorChannelMapping> m_mappings = null;

		/// <summary>
		/// Convert LOR Sequence data into a Vixen 3 sequence
		/// </summary>
		/// <param name="LightOramaSequence"></param>
		/// <param name="list"></param>
		public Vixen3SequenceCreator(LightOramaSequenceData LightOramaSequence, List<LorChannelMapping> mappings)
		{
			m_parsedLightOramaSequence = LightOramaSequence;
			m_mappings = mappings;

			m_conversionProgressBar = new CoversionProgressForm();
			m_conversionProgressBar.Show();

			m_conversionProgressBar.SetupProgressBar(0, m_parsedLightOramaSequence.mappings.Count);

			m_conversionProgressBar.StatusLineLabel = "Converting Light-O-Rama sequence";

			createTimedSequence();
			importSequenceData();

			m_conversionProgressBar.Close();
		} // Vixen3SequenceCreator

		/// <summary>
		/// Create a blank timed sequence and bind an audio track to it.
		/// </summary>
		private void createTimedSequence()
		{
			Sequence = new TimedSequence() { SequenceData = new TimedSequenceData() };

			// TODO: use this mark collection (maybe generate a grid?)
			//I am not sure what to do with this, but it looks like John had a plan.
			MarkCollection mc = new MarkCollection();

			Sequence.Length = TimeSpan.FromMilliseconds(m_parsedLightOramaSequence.SeqLengthInMills);

			var songFileName = m_parsedLightOramaSequence.SongPath + Path.DirectorySeparatorChar + m_parsedLightOramaSequence.SongFileName;
			// do we have an audio file specified
			if (songFileName != null)
			{
				// does the audio file exist?
				if (File.Exists(songFileName))
				{
					// use it
					Sequence.AddMedia(MediaService.Instance.GetMedia(songFileName));
				}
				else
				{
					var message = string.Format("Could not locate the audio file '{0}'; please add it manually " +
												"after import (Under Tools -> Associate Audio).", Path.GetFileName(songFileName));
					MessageBox.Show(message, "Couldn't find audio");
				} // audio file not found
			} // Audio file was specified
		} // createTimedSequence

		/// <summary>
		/// Convert parsedLightOramaSequence into a V3 sequence
		/// </summary>
		private void importSequenceData()
		{
			// for each channel in the LightOrama sequence
			foreach (LorChannelMapping channelMapping in m_mappings.Where(x => x.ColorMixing == false))
			{
				m_conversionProgressBar.IncrementProgressBar();
				Application.DoEvents();

				// is this channel defined in the LOR channel list?
				if (false == m_parsedLightOramaSequence.SequenceObjects.ContainsKey(Convert.ToUInt64(channelMapping.ChannelNumber)))
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

				LorChannel lorChannel = m_parsedLightOramaSequence.SequenceObjects[Convert.ToUInt64(channelMapping.ChannelNumber)] as LorChannel;
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
					if (null != node)
					{
						Sequence.InsertData(node);
					}
				} // end translate the effect
			} // end process each channel

			// now import the color mixing channels
			importSequenceDataRGB();
		} // end importSequenceData

		/// <summary>
		/// Convert parsedLightOramaSequence into a V3 sequence
		/// </summary>
		private void importSequenceDataRGB()
		{
			// for each color mixing channel in the LightOrama sequence
			foreach (LorChannelMapping channelMapping in m_mappings.Where(x => x.ColorMixing == true))
			{
				m_conversionProgressBar.IncrementProgressBar();
				Application.DoEvents();

				// is this channel defined in the LOR channel list?
				if (false == m_parsedLightOramaSequence.SequenceObjects.ContainsKey(Convert.ToUInt64(channelMapping.ChannelNumber)))
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

				LorChannel lorChannel = m_parsedLightOramaSequence.SequenceObjects[Convert.ToUInt64(channelMapping.ChannelNumber)] as LorChannel;
				LorRgbChannel lorRgbChannel = m_parsedLightOramaSequence.SequenceObjects[lorChannel.Parents.Single()] as LorRgbChannel;
				ElementNode vixElement = VixenSystem.Nodes.GetElementNode(channelMapping.ElementNodeId);
				if (null == vixElement)
				{
					Logging.Error("Vixen Element " + channelMapping.ElementNodeId + " could not be located.");
					continue;
				} // end could not locate the V3 element

				if ("Mega Tree 1-2 A22 p11" == lorRgbChannel.Name)
				{
					MessageBox.Show("GotIt");
				}
				// translate each effect associated with this channel
				lorRgbChannel.ConsolidateEffects(Sequence, vixElement);
			} // end process each channel
		} // end importSequenceDataRGB
	} // Vixen3SequenceCreator
} // VixenModules.SequenceType.LightOrama