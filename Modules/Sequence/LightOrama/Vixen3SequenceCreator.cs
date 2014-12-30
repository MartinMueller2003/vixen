using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vixen.Sys;
using VixenModules.Sequence.Timed;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Vixen.Services;
using Vixen.Module.Effect;
using VixenModules.App.Curves;
using VixenModules.App.ColorGradients;
using VixenModules.Effect.Pulse;
using VixenModules.Effect.SetLevel;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	public class PulseSummary
	{
		public Curve m_Curve = null;
		public ColorGradient m_ColorGradient = null;
		public TimeSpan m_StartTime = TimeSpan.FromMilliseconds(0);
		public TimeSpan m_EndTime = TimeSpan.FromMilliseconds(0);

		public static bool operator ==(PulseSummary lhPulseSummary, PulseSummary rhPulseSummary)
		{
			return ((lhPulseSummary.m_StartTime == rhPulseSummary.m_StartTime) &&
					(lhPulseSummary.m_EndTime == rhPulseSummary.m_EndTime) &&
					(lhPulseSummary.m_Curve.Points.SequenceEqual(rhPulseSummary.m_Curve.Points)) &&
					(lhPulseSummary.m_ColorGradient.Colors.SequenceEqual(rhPulseSummary.m_ColorGradient.Colors)));
		} // operator ==

		// The C# compiler and rule OperatorsShouldHaveSymmetricalOverloads require this.
		public static bool operator !=(PulseSummary lhPulseSummary, PulseSummary rhPulseSummary)
		{
			return !(lhPulseSummary == rhPulseSummary);
		} // operator !=

		public override bool Equals(object obj)
		{
			if ((null == obj) || (obj.GetType() != typeof(PulseSummary)))
				return false;

			PulseSummary p = (PulseSummary)obj;
			return (this == p);
		} // bool Equals

		public override int GetHashCode()
		{
			int result = 0;
			// result ^= Convert.ToInt32(m_StartTime.TotalMilliseconds);
			// result ^= Convert.ToInt32(m_EndTime.TotalMilliseconds);
			// result ^= m_Curve.GetHashCode();
			// result ^= m_ColorGradient.GetHashCode();

			return result;
		} // GetHashCode

	} // end PulseSummary

	public class Vixen3SequenceCreator
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ISequence Sequence { get; set; }

		private CoversionProgressForm m_conversionProgressBar = null;
		private CoversionProgressForm m_compressionProgressBar = null;
		private LightOramaSequenceData m_parsedLightOramaSequence = null;
		private List<LorChannelMapping> m_mappings = null;
		private TimeSpan m_emptyTimeSpan = new TimeSpan(0);
		private TimeSpan m_maxGapTimeSpan = new TimeSpan(0, 0, 0, 0, 25);
		private const double m_minIntensityChange = 5;
		private const int SET_LEVEL_INTENSITY_PARAMETER_INDEX = 0;
		private const int SET_LEVEL_COLOR_PARAMETER_INDEX = 1;
		private const int PULSE_CURVE_PARAMETER_INDEX = 0;
		private const int PULSE_COLOR_PARAMETER_INDEX = 1;
		//		private const int MAX_MS_GAP = 1;


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

			// list of parent elements. Used by the consolidation functions
			List<EffectNode> listOfEffects = new List<EffectNode>();

			// get a list of unique destinations
			List<LorChannelMapping> elementMappings = m_mappings.Where(x => x.ElementNodeId != Guid.Empty).GroupBy(x => x.ElementNodeId).Select(g => g.First()).ToList();

			m_conversionProgressBar.SetupProgressBar(0, elementMappings.Count());

			int currentMappingNum = 0;
			foreach (LorChannelMapping elementMapping in elementMappings)
			{
				m_conversionProgressBar.UpdateProgressBar(currentMappingNum++);
				Application.DoEvents();

				ElementNode vixenElement = VixenSystem.Nodes.GetElementNode(elementMapping.ElementNodeId);
				if (null == vixenElement)
				{
					Logging.Error("Vixen Element " + elementMapping.ElementNodeId + " could not be located for mapping " + elementMapping.ChannelName);
					errorCount++;
					continue;
				}

				// get a list of the channels mapped to this element
				IEnumerable<LorChannelMapping> channelMappings = m_mappings.Where(x => x.ElementNodeId == elementMapping.ElementNodeId).ToList();

				// build the list of effects assigned to this element
				listOfEffects.AddRange(ProcessEffects(vixenElement, channelMappings));

			} // end process each mapped element

			// parse the effects and combine as many as possible to a parent element
			listOfEffects = PostProcessConcurrentEffects(listOfEffects);

			// add the effects for this parent
			Sequence.InsertData(listOfEffects);

		} // importSequenceData

		/// <summary>
		/// Examine the effects on channels and convert them to vixen effects. This results in an unoptimized list of bulk events.
		/// </summary>
		/// <param name="vixElement">The V3 Element to which the effects will be attached</param>
		/// <param name="channelMappings">List of the input channels contributing color to the element</param>
		public List<EffectNode> ProcessEffects(ElementNode vixElement, IEnumerable<LorChannelMapping> channelMappings)
		{
			List<EffectNode> listOfEffects = new List<EffectNode>();

			// process the effects for each contributing channel
			foreach (LorChannelMapping sourceChannelMapping in channelMappings)
			{
				// is this channel present in the list of channel numbers?
				if (false == m_parsedLightOramaSequence.SequenceObjects.ContainsKey(sourceChannelMapping.ChannelNumber))
				{
					continue;
				}

				// get the LOR channel
				ILorObject lorObject = m_parsedLightOramaSequence.SequenceObjects[sourceChannelMapping.ChannelNumber];
				if (null == lorObject)
				{
					// failed to get the channel data (can happen when using a profile)
					continue;
				}

				// translate and add to the list of effects
				listOfEffects.AddRange(lorObject.TranslateEffects(vixElement, sourceChannelMapping.DestinationColor));
			} // end for each child 

			return listOfEffects;
		} // ProcessEffects

		/// <summary>
		/// Find concurrent effects where the properties are identical and see if they can be assigned to multiple elements
		/// </summary>
		/// <param name="listOfEffects"></param>
		/// <returns>listOfEffects</returns>
		private List<EffectNode> PostProcessConcurrentEffects(List<EffectNode> startingListOfEffects)
		{
			// ignore the non pulse effects
			List<EffectNode> listOfEffects = startingListOfEffects.Where(x => x.Effect.GetType() != typeof(Pulse)).ToList();
			List<EffectNode> listOfPulseEffects = startingListOfEffects.Where(x => x.Effect.GetType() == typeof(Pulse)).ToList();

			int currentEffectNum = 1;
			m_compressionProgressBar = new CoversionProgressForm();
			m_compressionProgressBar.Show();
			m_compressionProgressBar.SetupProgressBar(0, listOfPulseEffects.Count + 1);
			m_compressionProgressBar.StatusLineLabel = "Compressing Light-O-Rama sequence";

			Dictionary<PulseSummary, EffectNode> effectSummaryList = new Dictionary<PulseSummary, EffectNode>();

			// create a summary of the effects and elements using common effects
			foreach (EffectNode pulseEffect in listOfPulseEffects)
			{
				m_compressionProgressBar.UpdateProgressBar(currentEffectNum++);
				Application.DoEvents();

				PulseSummary pulseSummary = new PulseSummary();
				pulseSummary.m_Curve = (pulseEffect.Effect as Pulse).ParameterValues[PULSE_CURVE_PARAMETER_INDEX] as Curve;
				pulseSummary.m_ColorGradient = (pulseEffect.Effect as Pulse).ParameterValues[PULSE_COLOR_PARAMETER_INDEX] as ColorGradient;
				pulseSummary.m_StartTime = pulseEffect.StartTime;
				pulseSummary.m_EndTime = pulseEffect.EndTime;

				// does this effect already exist in the list of effects?
				if (false == effectSummaryList.ContainsKey(pulseSummary))
				{
					// create an instance for this effect
					effectSummaryList.Add(pulseSummary, pulseEffect);
					listOfEffects.Add(pulseEffect);
				} // end create new effect entry
				else
				{

					// add the effect target to the list of targets for this effect
					List<ElementNode> existingTargetList = (effectSummaryList[pulseSummary].Effect as Pulse).TargetNodes.ToList();
					existingTargetList.AddRange((pulseEffect.Effect as Pulse).TargetNodes.ToList());
					(effectSummaryList[pulseSummary].Effect as Pulse).TargetNodes = existingTargetList.ToArray();
				} // end add additional target
			} // end bin each effect

			m_compressionProgressBar.Close();

			return listOfEffects;
		} // PostProcessConcurrentEffects
	} // Vixen3SequenceCreator
} // VixenModules.SequenceType.LightOrama