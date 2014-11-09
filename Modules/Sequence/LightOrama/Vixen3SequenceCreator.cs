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
	public class Vixen3SequenceCreator
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ISequence Sequence { get; set; }

		private CoversionProgressForm m_conversionProgressBar = null;
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

			// get a list of unique destinations
			IEnumerable<LorChannelMapping> elementMappings = m_mappings.Where(x => x.ElementNodeId != Guid.Empty).GroupBy(x => x.ElementNodeId).Select(g => g.First()).ToList();

			m_conversionProgressBar.SetupProgressBar(0, elementMappings.Count());

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

				List<EffectNode> listOfEffects = new List<EffectNode>();

				ProcessEffects(vixElement, channelMappings, ref listOfEffects);

				if ("String 1 3" == vixElement.Name)
				{
					listOfEffects = listOfEffects.OrderBy(x => x.StartTime).ToList();
				}


				// try to consolidate some of the effects
				PostProcessListOfEffects(listOfEffects, elementMapping.ColorMixing);

				// update the sequence with these effects
				Sequence.InsertData(listOfEffects);
			} // end process each mapped element
		} // importSequenceData

		/// <summary>
		/// Examine the effects on channels and convert them to vixen effects.
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="channelMappings"></param>
		/// <param name="listOfEffects"></param>
		public void ProcessEffects(ElementNode vixElement, IEnumerable<LorChannelMapping> channelMappings, ref List<EffectNode> listOfEffects)
		{
			// process the effects for each child
			foreach (LorChannelMapping sourceChannelMapping in channelMappings)
			{
				if (false == m_parsedLightOramaSequence.SequenceObjects.ContainsKey(sourceChannelMapping.ChannelNumber))
				{
					continue;
				}

				LorChannel lorChannel = m_parsedLightOramaSequence.SequenceObjects[sourceChannelMapping.ChannelNumber] as LorChannel;

				if (null == lorChannel)
				{
					continue;
				}

				foreach (ILorEffect sampleEffect in lorChannel.Effects)
				{
					EffectNode effect = null;
					if (null != (effect = sampleEffect.translateEffect(vixElement, sourceChannelMapping.DestinationColor)))
					{
						listOfEffects.Add(effect);
					}
				} // end list of effects
			} // end for each child 
		} // ConsolidateEffects

		/// <summary>
		/// Combine the individuale effects assigned to the vixen element to reduce the number of descrete effects.
		/// </summary>
		/// <param name="listOfEffects"></param>
		/// <param name="colorMixing"></param>
		private void PostProcessListOfEffects(List<EffectNode> listOfEffects, bool colorMixing)
		{
			// combine any contiguous effects
			PostProcessListOfContiguousEffects(listOfEffects);

			// this must be done after the combination of contiguous effects into a single effect
			if (true == colorMixing)
			{
				// combine multiple concurrent effects into a single effect
				PostProcessPulseSetLevelConcurrentEffects(listOfEffects);
			} // end mix colors as needed
		} // PostProcessListOfEffects

		/// <summary>
		/// Find concurrent effects where the start and end times align and see if they can be combined
		/// </summary>
		/// <param name="listOfEffects"></param>
		/// <returns>listOfEffects</returns>
		private void PostProcessPulseSetLevelConcurrentEffects(List<EffectNode> listOfEffects)
		{
			// Process each distinct start time
			IEnumerable<EffectNode> listOfEffectStartTimes = listOfEffects.GroupBy(x => x.StartTime).Select(g => g.First()).ToList();
			foreach (EffectNode effectStartTime in listOfEffectStartTimes)
			{
				// process each distinct end time that shares this start time
				IEnumerable<EffectNode> listOfEffectEndTimes = listOfEffects.Where(x => x.StartTime == effectStartTime.StartTime).GroupBy(x => x.EndTime).Select(g => g.First()).ToList();
				foreach (EffectNode effectEndTime in listOfEffectEndTimes)
				{
					// build a list of concurrent effects and process it
					IEnumerable<EffectNode> listOfConcurrentEffects = listOfEffects.Where(x => (x.StartTime == effectStartTime.StartTime) && (x.EndTime == effectEndTime.EndTime)).ToList();
					if (2 > listOfConcurrentEffects.Count())
					{
						// just leave these effects alone. There is not much we can do with them
						continue;
					} // end nothing to combine

					int startIntensity = 0;
					int endIntensity = 0;
					int startRed = 0;
					int startGreen = 0;
					int startBlue = 0;
					int endRed = 0;
					int endGreen = 0;
					int endBlue = 0;
					TimeSpan pulseStartTimeSpan = effectStartTime.StartTime;
					TimeSpan pulseEndTimeSpan = m_emptyTimeSpan;

					// process each of the concurrent effects
					foreach (EffectNode concurrentEffect in listOfConcurrentEffects)
					{
						Color effectStartColor = Color.Empty;
						Color effectEndColor = Color.Empty;
						double effectStartIntensity = 0.0;
						double effectEndIntensity = 0.0;

						// is this a pulse?
						if (typeof(Pulse) == concurrentEffect.Effect.GetType())
						{
							Curve curve = concurrentEffect.Effect.ParameterValues[PULSE_CURVE_PARAMETER_INDEX] as Curve;
							ColorGradient colorGradient = concurrentEffect.Effect.ParameterValues[PULSE_COLOR_PARAMETER_INDEX] as ColorGradient;

							// make sure this curve has only a single pair of points
							if ((2 != curve.Points.Count) || (1 != colorGradient.Colors.Count))
							{
								// dont know how to handle this. Leave it alone.
								continue;
							} // end filter for non ramps

							// get the start and end intensity
							effectStartIntensity = curve.Points.First().Y / 100.0;
							effectEndIntensity = curve.Points.Last().Y / 100.0;
							effectStartColor = colorGradient.GetColorAt(0.0);
							effectEndColor = colorGradient.GetColorAt(100.0);

							// show that we have some time
							pulseEndTimeSpan = concurrentEffect.EndTime;

						} // end process a pulse effect

						else if (typeof(SetLevel) == concurrentEffect.Effect.GetType())
						{
							effectStartColor = effectEndColor = (Color)concurrentEffect.Effect.ParameterValues[PULSE_COLOR_PARAMETER_INDEX];
							effectStartIntensity = effectEndIntensity = (double)concurrentEffect.Effect.ParameterValues[SET_LEVEL_INTENSITY_PARAMETER_INDEX];

							// show that we have some time
							pulseEndTimeSpan = concurrentEffect.EndTime;
						} // end process Set Level effect
						else
						{
							// This is not an effect we can combine.
							continue;
						} // end uncombinable effect

						// adjust the color at the start and end of the ramp
						startRed = Math.Max(startRed, Convert.ToInt32(effectStartColor.R * effectStartIntensity));
						startGreen = Math.Max(startGreen, Convert.ToInt32(effectStartColor.G * effectStartIntensity));
						startBlue = Math.Max(startBlue, Convert.ToInt32(effectStartColor.B * effectStartIntensity));

						endRed = Math.Max(endRed, Convert.ToInt32(effectEndColor.R * effectEndIntensity));
						endGreen = Math.Max(endGreen, Convert.ToInt32(effectEndColor.G * effectEndIntensity));
						endBlue = Math.Max(endBlue, Convert.ToInt32(effectEndColor.B * effectEndIntensity));

						startIntensity = Math.Min(100, Math.Max(startIntensity, Convert.ToInt32(effectStartIntensity * 100)));
						endIntensity = Math.Min(100, Math.Max(endIntensity, Convert.ToInt32(effectEndIntensity * 100)));

						// now remove the effect we have processed from the list of effects
						listOfEffects.Remove(concurrentEffect);

					} // end process concurrent effects

					// did we find any effects to combine?
					if (m_emptyTimeSpan == pulseEndTimeSpan)
					{
						// nope
						continue;
					} // end empty conversion check

					Color startColor = Color.FromArgb(255, startRed, startGreen, startBlue);
					Color endColor = Color.FromArgb(255, endRed, endGreen, endBlue);

					// allocate a pulse effect Module
					IEffectModuleInstance pulseInstance = ApplicationServices.Get<IEffectModuleInstance>(new PulseDescriptor().TypeId);
					if (null == pulseInstance)
					{
						Logging.Error("PostProcessPulseSetLevelConcurrentEffects: Could not allocate an instance of IEffectModuleInstance");
						break;
					} // end could not allocate a pulse instance

					// Clone() Doesn't work! :(
					pulseInstance.TargetNodes = effectStartTime.Effect.TargetNodes.ToArray();
					pulseInstance.TimeSpan = (pulseEndTimeSpan - pulseStartTimeSpan).Duration();
					EffectNode newEeffectNode;
					if (null == (newEeffectNode = new EffectNode(pulseInstance, pulseStartTimeSpan)))
					{
						// could not allocate the structure
						Logging.Error("PostProcessPulseSetLevelConcurrentEffects: Could not allocate an instance of EffectNode");
						break;
					} // end could not allocate an effect instance

					PointPairList pointPairList = new PointPairList();
					double durration = (pulseEndTimeSpan - pulseStartTimeSpan).Duration().TotalMilliseconds;

					// add a point for the starting and ending time of this set level effect
					pointPairList.Add(0.0, startIntensity);
					pointPairList.Add(100.0, endIntensity);

					// build a list of color points for the gradient
					ColorGradient cg = new ColorGradient();
					cg.Colors.Clear();
					cg.Colors.Add(new ColorPoint(startColor, 0.0));
					cg.Colors.Add(new ColorPoint(endColor, 1.0));

					// fill in the pusle parameters
					newEeffectNode.Effect.ParameterValues = new Object[]
					{
						new Curve(pointPairList), new ColorGradient(cg)
					};

					// Add the result to the output list
					listOfEffects.Add(newEeffectNode);
				} // end process concurrent effects
			} // end process common start times
		} // PostProcessPulseSetLevelConcurrentEffects

		/// <summary>
		/// LOR creates many single color set level effects of short durration that are contiguous in time but vary slightly in intensity.
		/// These can be combined into a multi point curved pulse effect, saving memory.
		/// </summary>
		/// <param name="listOfEffects"></param>
		/// <returns></returns>
		private void PostProcessListOfContiguousEffects(List<EffectNode> listOfEffects)
		{
			// generate a list of distinct color set level events
			IEnumerable<EffectNode> listOfPulseEffects = listOfEffects.Where(x => x.GetType() == typeof(Pulse)).ToList();
//			IEnumerable<EffectNode> listOfColors = listOfPulseEffects.GroupBy(x => (x.Effect.ParameterValues[SET_LEVEL_COLOR_PARAMETER_INDEX] as ColorGradient).Colors.First()).Select(g => g.First()).ToList();
			Dictionary<Color, List<EffectNode>> listOfColorLists = new Dictionary<Color, List<EffectNode>>();

			// sort the effects based on the initial color
			foreach (EffectNode effect in listOfEffects)
			{
				ColorGradient colorGradient = effect.Effect.ParameterValues[PULSE_COLOR_PARAMETER_INDEX] as ColorGradient;
				Color startColor = colorGradient.GetColorAt(0.0);
				Color endColor = colorGradient.GetColorAt(1.0);

				// is this a mono color effect?
				if( startColor != endColor )
				{
					// only process mono color effects
					continue;
				} // end mono color check

				// is there a list for this color?
				if( false == listOfColorLists.ContainsKey(startColor))
				{
					// make one
					listOfColorLists.Add(startColor, new List<EffectNode>());
				}

				// add this effect node to the proper list
				listOfColorLists[startColor].Add(effect);
			} // end sort into color bins

			// we now have the effects sorted into lists of colors. Now process each color.
			foreach( var listOfEffectsForThisColor in listOfColorLists)
			{
				// do we have more than one effect to combine?
				if( 2 > listOfEffectsForThisColor.Value.Count)
				{
					// just ignore this color
					continue;
				}

				IEnumerable<EffectNode> sortedListOfEffectsForThisColor = listOfEffectsForThisColor.Value.OrderBy(x => x.StartTime).ToList();

				TimeSpan pulseStartTime = m_emptyTimeSpan;
				TimeSpan pulseEndTime = m_emptyTimeSpan;

				List<EffectNode> currentPulseEffects = new List<EffectNode>();

				Color color = listOfEffectsForThisColor.Key;

				// process each effect
				foreach (EffectNode currentEffect in sortedListOfEffectsForThisColor)
				{
					// this is where things get interesting. We need to find set level effects of the same color that butt up against each other 
					// and combine them into a single pulse effect using a curve for the different intensities. 
					// NOTE: Since points are expressed as a percentage of the total length of the pulse, 
					//       we cant create points until we know how long the pulse will be.

					// is this the first effect in the pulse?
					if (m_emptyTimeSpan == pulseStartTime)
					{
						// starting a new pulse
						currentPulseEffects.Add(currentEffect);
						pulseStartTime = currentEffect.StartTime;
						pulseEndTime = currentEffect.EndTime;
					}
					else if (m_maxGapTimeSpan < (currentEffect.StartTime - pulseEndTime).Duration())
					{
						// close the current pulse
						CreatePulseEffect(currentPulseEffects, pulseStartTime, pulseEndTime, listOfEffects, color);

						// Start a new pulse effect
						currentPulseEffects.Clear();
						pulseStartTime = currentEffect.StartTime;
						pulseEndTime = currentEffect.EndTime;
						currentPulseEffects.Add(currentEffect);
					}
					else
					{
						// add the set level to the current pulse
						currentPulseEffects.Add(currentEffect);
						if (pulseEndTime < currentEffect.EndTime)
						{
							pulseEndTime = currentEffect.EndTime;
						}
					} // end expand the current pulse
				} // end for each instance of a set level effect

				// Close the final event?
				CreatePulseEffect(currentPulseEffects, pulseStartTime, pulseEndTime, listOfEffects, color);
			} // end for each color
		} // PostProcessListOfSetLevelEffects

		/// <summary>
		/// Generate a pulse effect for the list of related LOR effects
		/// </summary>
		/// <param name="currentEffects"></param>
		/// <param name="pulseStartTime"></param>
		/// <param name="pulseEndTime"></param>
		/// <param name="finalListOfEffects"></param>
		/// <param name="color"></param>
		private void CreatePulseEffect(List<EffectNode> currentEffects,
										TimeSpan pulseStartTime,
										TimeSpan pulseEndTime,
										List<EffectNode> finalListOfEffects,
										Color color)
		{
			do
			{
				// is there anything to combine??
				if (2 > currentEffects.Count)
				{
					// just go away
					break;
				} // end list is empty

				// we get to this point if we need to add a pulse effect to replace the existing effects

				// allocate the effect Module
				IEffectModuleInstance pulseInstance = ApplicationServices.Get<IEffectModuleInstance>(new PulseDescriptor().TypeId);
				if (null == pulseInstance)
				{
					Logging.Error("CreatePulseEffect: Could not allocate an instance of IEffectModuleInstance");
					break;
				} // end could not allocate

				// Clone() Doesn't work! :(
				pulseInstance.TargetNodes = currentEffects.First().Effect.TargetNodes.ToArray();
				pulseInstance.TimeSpan = (pulseEndTime - pulseStartTime).Duration();
				EffectNode newEeffectNode = new EffectNode(pulseInstance, pulseStartTime);
				if (null == newEeffectNode)
				{
					// could not allocate the structure
					Logging.Error("CreatePulseEffect: Could not allocate an instance of EffectNode");
					break;
				} // end could not allocate

				PointPairList pointPairList = new PointPairList();
				TimeSpan lastEffectdEndTime = m_emptyTimeSpan;
				double durration = (pulseEndTime - pulseStartTime).TotalMilliseconds;

				// build a list of points for this pulse effect
				foreach (EffectNode effectNode in currentEffects)
				{
					// does this effect overlap the previous effect?
					if (effectNode.StartTime <= lastEffectdEndTime)
					{
						// make sure they do not overlap
						effectNode.StartTime = lastEffectdEndTime + new TimeSpan(1);
					} // end adjust start time

					double effectStartPercent = Math.Min( 100.0,((effectNode.StartTime - pulseStartTime).TotalMilliseconds / durration) * 100.0);
					double effectEndPercent = Math.Min(100.0, ((effectNode.EndTime - pulseStartTime).TotalMilliseconds / durration) * 100.0);
					double effectDurrationPercent = effectEndPercent - effectStartPercent;

					// process points in the curve
					Curve curve = effectNode.Effect.ParameterValues[0] as Curve;
					foreach (PointPair point in curve.Points)
					{
						double start = effectStartPercent + ((point.X / 100) * effectDurrationPercent);
						if (2 > pointPairList.Count)
						{
							// add a new point
							pointPairList.Add(start, point.Y);
						}
						// is the new intensity significantly different than the previous intensity?
						else if (m_minIntensityChange < Math.Abs(point.Y - pointPairList.Last().Y))
						{
							// add a new point
							pointPairList.Add(start, point.Y);
						}
						else
						{
							// update the previous point
							pointPairList.Last().X = start;
						}
					} // end process each point in the pulse

					// remove this effect from the master list of effects
					finalListOfEffects.Remove(effectNode);
				} // end process each set level effect

				// fill in the pusle parameters
				newEeffectNode.Effect.ParameterValues = new Object[]
				{
					new Curve(pointPairList), new ColorGradient(color)
				};

				// Add the result to the output list
				finalListOfEffects.Add(newEeffectNode);

			} while (false);
		} // CreatePulseEffect
	} // Vixen3SequenceCreator
} // VixenModules.SequenceType.LightOrama