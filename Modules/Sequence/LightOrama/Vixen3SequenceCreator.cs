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

			m_conversionProgressBar.SetupProgressBar(0, m_parsedLightOramaSequence.Mappings.Count);

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
		/// Convert the LOR sequence to a vixen sequence based on the current mappings
		/// </summary>
		private void importSequenceData()
		{
			int errorCount = 0;

			// get a list of unique destinations
			IEnumerable<LorChannelMapping> elementMappings = m_mappings.Where(x => x.ElementNodeId != Guid.Empty).GroupBy(x => x.ElementNodeId).Select(g => g.First()).ToList();

			m_conversionProgressBar.SetupProgressBar(0, elementMappings.ToList().Count);

			int currentMappingNum = 0;
			foreach (LorChannelMapping elementMapping in elementMappings)
			{
				m_conversionProgressBar.UpdateProgressBar(currentMappingNum++);
				Application.DoEvents();

				ElementNode vixElement = VixenSystem.Nodes.GetElementNode(elementMapping.ElementNodeId);
				if (null == vixElement)
				{
					Logging.Error("Vixen Element " + elementMapping.ElementNodeId + " could not be located for mapping " + elementMapping.ChannelName);
					errorCount++;
					continue;
				}

				// get a list of the channels mapped to this element
				IEnumerable<LorChannelMapping> channelMappings = m_mappings.Where(x => x.ElementNodeId == elementMapping.ElementNodeId).ToList();

				// process the effects on these channels
				if( true == elementMapping.ColorMixing)
				{
					ConsolidateEffects(vixElement, channelMappings);
				}
				else
				{
					ProcessEffects(vixElement, channelMappings);
				}
			} // end process each mapped element
		} // importSequenceData

		/// <summary>
		/// Examine the effects on the non color mixing channels and convert them.
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="channelMappings"></param>
		public void ProcessEffects(ElementNode vixElement, IEnumerable<LorChannelMapping> channelMappings)
		{
			// process the effects for each child
			foreach (LorChannelMapping sourceChannelMapping in channelMappings)
			{
				LorChannel lorChannel = m_parsedLightOramaSequence.SequenceObjects[sourceChannelMapping.ChannelNumber] as LorChannel;
				foreach (ILorEffect sampleEffect in lorChannel.Effects.Where(x => (false == x.HasBeenProcessed) && (x.StartTimeMs < x.EndTimeMs)))
				{
					Sequence.InsertData(sampleEffect.translateEffect(vixElement, sourceChannelMapping.DestinationColor));
				} // end list of effects
			} // end for each child 
		} // ConsolidateEffects

		/// <summary>
		/// Examine the effects on the color mixing channels and consolidate them.
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="channelMappings"></param>
		public void ConsolidateEffects(ElementNode vixElement, IEnumerable<LorChannelMapping> channelMappings)
		{
			// process the effects for each child
			foreach (LorChannelMapping sourceChannelMapping in channelMappings)
			{
				LorChannel lorChannel = m_parsedLightOramaSequence.SequenceObjects[sourceChannelMapping.ChannelNumber] as LorChannel;
				foreach (ILorEffect sampleEffect in lorChannel.Effects.Where(x => (false == x.HasBeenProcessed) && (x.StartTimeMs < x.EndTimeMs)))
				{
					List<ILorEffect> effectList = new List<ILorEffect>();

					// process this effect for each channel
					foreach (LorChannelMapping effectSourceChannelMapping in channelMappings)
					{
						LorChannel effectSourceChannel = m_parsedLightOramaSequence.SequenceObjects[effectSourceChannelMapping.ChannelNumber] as LorChannel;
						// find peer effects
						foreach (ILorEffect effect in effectSourceChannel.Effects.Where(x => (false == x.HasBeenProcessed) && (x.StartTimeMs == sampleEffect.StartTimeMs) && (x.EndTimeMs == sampleEffect.EndTimeMs) && (x.RampUp == sampleEffect.RampUp) && (x.RampDown == sampleEffect.RampDown) && (x.GetType() == sampleEffect.GetType())))
						{
							// add the color to the effect for mixing
							effect.Color = effectSourceChannel.Color;
							effectList.Add(effect);
						} // end collect peer effects
					} // end collect effect from a child

					ILorEffect finalEffect = effectList.First().CombineEffects(effectList);
					Sequence.InsertData(finalEffect.translateEffect(vixElement, finalEffect.Color));
				} // end main list of effects
			} // end for each child 
		} // ConsolidateEffects
	} // Vixen3SequenceCreator
} // VixenModules.SequenceType.LightOrama