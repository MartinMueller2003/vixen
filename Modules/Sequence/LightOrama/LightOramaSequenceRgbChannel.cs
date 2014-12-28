using NLog;
using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Vixen.Data.Flow;
using Vixen.Module.Effect;
using Vixen.Module.OutputFilter;
using Vixen.Module.Property;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using VixenModules.App.Curves;
using VixenModules.App.ColorGradients;
using VixenModules.Effect.Pulse;
using VixenModules.Effect.SetLevel;
using VixenModules.OutputFilter.ColorBreakdown;
using VixenModules.Property.Color;
using VixenModules.Sequence.Timed;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	public class LorRgbChannel : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		private const int SET_LEVEL_INTENSITY_PARAMETER_INDEX = 0;
		private const int SET_LEVEL_COLOR_PARAMETER_INDEX = 1;
		private const int PULSE_CURVE_PARAMETER_INDEX = 0;
		private const int PULSE_COLOR_PARAMETER_INDEX = 1;
		private const double m_minIntensityChange = 5;


		public string Name { get; set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public List<ILorEffect> Effects { get; private set; }
		public Guid ElementId { get; private set; }

		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorRgbChannel(XElement element, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			Children = new List<UInt64>();
			Parents = new List<UInt64>();
			Name = string.Empty;
			Index = 0;
			TotalMs = 0;
			m_sequenceObjects = sequenceObjects;
			ElementId = Guid.Empty;
			Parse(element);
		} // LorRgbChannel

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
					case "channels":
						foreach (XElement rgbChannel in element.Elements("channel").ToList())
						{
							UInt64 childIndex = (null == rgbChannel.Attribute("savedIndex")) ? UInt64.MaxValue : UInt64.Parse(rgbChannel.Attribute("savedIndex").Value);

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
						Logging.Error("Skipping unsupported LOR sequence Element ...rgbChannel.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse

		/// <summary>
		/// create this element in a tree of elements. Create any parents as needed
		/// </summary>
		/// <param name="sequence"></param>
		public void CreateVixenElement(LightOramaSequenceData sequence)
		{
			do
			{
				// do we already have an element ID
				if (Guid.Empty != ElementId)
				{
					// just go away
					break;
				} // end filter creation of element

				// create an element for this lor object
				ElementNode element = ElementNodeService.Instance.CreateSingle(null, Name);
				ElementId = element.Id;

				// Bind to the parent nodes.
				foreach (UInt64 parentId in Parents)
				{
					ILorObject parentObject = sequence.SequenceObjects[parentId];
					parentObject.CreateVixenElement(sequence);

					// bind the parent to this node
					ElementNode parentElement = VixenSystem.Nodes.GetElementNode(parentObject.ElementId);
					VixenSystem.Nodes.AddChildToParent(element, parentElement);
				} // end bind to parents

				// check to see if we should be at the root level?
				if (0 != Parents.Count)
				{
					VixenSystem.Nodes.RemoveNode(element, null, true);
				}

				// get the color handling property
				ColorModule colorProperty = element.Properties.Add(ColorDescriptor.ModuleId) as ColorModule;

				colorProperty.ColorType = ElementColorType.FullColor;
				colorProperty.SingleColor = Color.Empty;
				colorProperty.ColorSetName = String.Empty;

				ColorBreakdownModule breakdown = ApplicationServices.Get<IOutputFilterModuleInstance>(ColorBreakdownDescriptor.ModuleId) as ColorBreakdownModule;
				VixenSystem.DataFlow.SetComponentSource(breakdown, new DataFlowComponentReference(VixenSystem.DataFlow.GetComponent(element.Element.Id), 0));
				VixenSystem.Filters.AddFilter(breakdown);

				// build the color outputs
				List<ColorBreakdownItem> newBreakdownItems = new List<ColorBreakdownItem>();

				// get the color order from the children
				foreach (UInt64 childId in Children)
				{
					LorChannel child = sequence.SequenceObjects[childId] as LorChannel;
					ColorBreakdownItem cbi = new ColorBreakdownItem();
					cbi.Color = child.Color;
					cbi.Name = child.Color.Name;
					newBreakdownItems.Add(cbi);
				} // end process child colors

				breakdown.MixColors = true;
				breakdown.BreakdownItems = newBreakdownItems;
			} while (false);
		} // CreateVixenElement

		/// <summary>
		/// Map the leaf objects to Vixen elements of the same name. RGB Channel does NOT add its children to the map.
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		public int addLorObjectToMap(List<LorChannelMapping> mappings)
		{
			return AddToMappings(mappings);
		} // addLorObjectToMap

		/// <summary>
		/// Update the mappings for this channel
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		public int AddToMappings(List<LorChannelMapping> mappings)
		{
			// get the mapping for this channel
			LorChannelMapping mapping = mappings.FirstOrDefault(x => x.ChannelName == Name);
			if (null == mapping)
			{
				// this is a new mapping
				mapping = new LorChannelMapping(Name,
												Color.Empty,
												Index,
												ElementId,
												Color.Empty,
												true);
				mappings.Add(mapping);
			}
			else
			{
				// update the data in the existing mapping
				mapping.DestinationColor = Color.Empty;
				mapping.ColorMixing = true;
				mapping.ElementNodeId = ElementId;
			}

			return 1;
		} // AddToMappings

		/// <summary>
		/// Translate the effects for this channel
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public IEnumerable<EffectNode> TranslateEffects(ElementNode vixElement, System.Drawing.Color color)
		{
			List<EffectNode> listOfEffects = new List<EffectNode>();

			// get the color order from the children
			foreach (UInt64 childId in Children)
			{
				listOfEffects.AddRange(m_sequenceObjects[childId].TranslateEffects(vixElement, (m_sequenceObjects[childId] as LorChannel).Color));
			} // end process child colors


			listOfEffects = TranslateSetLevelEffects(vixElement, listOfEffects).ToList();
			listOfEffects = TranslateConcurrentEffects(vixElement, listOfEffects).ToList();

			return listOfEffects;
		} // TranslateEffects

		/// <summary>
		/// Combine the pulse effects that happen concurrently
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="startingEffects">Mixed list of effects</param>
		/// <returns>Consolidated list of nodes</returns>
		private IEnumerable<EffectNode> TranslateConcurrentEffects(ElementNode vixElement, List<EffectNode> startingEffects)
		{
			List<EffectNode> listOfEffects = new List<EffectNode>();

			TimeSpan m_emptyTimeSpan = new TimeSpan(0);

			// Process each distinct start time
			IEnumerable<EffectNode> listOfPulseEffectStartTimes = startingEffects.GroupBy(x => x.StartTime).Select(g => g.First()).OrderBy(x => x.StartTime).ToList();
			foreach (EffectNode effectStartTime in listOfPulseEffectStartTimes)
			{
				// process each distinct end time that shares this start time
				IEnumerable<EffectNode> listOfPulseEffectEndTimes = startingEffects.Where(x => x.StartTime == effectStartTime.StartTime).GroupBy(x => x.EndTime).Select(g => g.First()).ToList();
				foreach (EffectNode effectEndTime in listOfPulseEffectEndTimes)
				{
					// build a list of concurrent effects and process it
					IEnumerable<EffectNode> listOfConcurrentPulseEffects = startingEffects.Where(x => (x.StartTime == effectStartTime.StartTime) && (x.EndTime == effectEndTime.EndTime)).ToList();
					if (2 > listOfConcurrentPulseEffects.Count())
					{
						// just leave these effects alone. There is not much we can do with them
						listOfEffects.AddRange(listOfConcurrentPulseEffects);
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
					foreach (EffectNode concurrentEffect in listOfConcurrentPulseEffects)
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
								listOfEffects.Add(concurrentEffect);
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
						Logging.Error("TranslatePulseEffects: Could not allocate an instance of IEffectModuleInstance");
						break;
					} // end could not allocate a pulse instance

					// Clone() Doesn't work! :(
					pulseInstance.TargetNodes = effectStartTime.Effect.TargetNodes.ToArray();
					pulseInstance.TimeSpan = (pulseEndTimeSpan - pulseStartTimeSpan).Duration();
					EffectNode newEffectNode;
					if (null == (newEffectNode = new EffectNode(pulseInstance, pulseStartTimeSpan)))
					{
						// could not allocate the structure
						Logging.Error("TranslatePulseEffects: Could not allocate an instance of EffectNode");
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
					newEffectNode.Effect.ParameterValues = new Object[]
					{
						new Curve(pointPairList), new ColorGradient(cg)
					};

					// Add the result to the output list
					listOfEffects.Add(newEffectNode);
				} // end process concurrent effects
			} // end process common start times
			return listOfEffects;
		} // TranslatePulseEffects

		/// <summary>
		/// Consolidate the Set Level effects into complex color effects
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="startingEffects">Mixed list of Set Level and Pulse Effects</param>
		/// <returns>Consolidated effects</returns>
		public IEnumerable<EffectNode> TranslateSetLevelEffects(ElementNode vixElement, List<EffectNode> startingEffects)
		{
			List<EffectNode> listOfEffects = startingEffects.Where(x => (x.Effect.GetType() != typeof(SetLevel))).OrderBy(x => x.StartTime).ToList();
			List<EffectNode> setLevelEffects = startingEffects.Where(x => (x.Effect.GetType() == typeof(SetLevel))).OrderBy(x => x.StartTime).ToList();

			TimeSpan TimeWindowSize = TimeSpan.FromMilliseconds(10);
			EffectNode pulseEffectNode = null;

			do
			{
				// skip to the end if we have no set level effects
				if (0 == setLevelEffects.Count)
				{
					break;
				} // no set level effect check

				// get the earliest possible start time
				TimeSpan startTime = setLevelEffects.First().StartTime;

				// find the latest possible end time
				TimeSpan endTime = setLevelEffects.OrderBy(x => x.EndTime).Last().EndTime;

				// Logging.Info("TranslateEffects: startTime: '" + startTime + "' endTime: '" + endTime + "'");

				// set a starting point for the first pulse
				TimeSpan pulseStartTime = startTime;
				TimeSpan endTimeWindow = startTime;

				// allocate a working point list (Color, Intensity, Time)
				PointPairList listOfPoints = new PointPairList();

				// init the last color
				Color lastColor = Color.FromArgb(0, 0, 0, 0);
				double lastIntensity = 0.0;

				// convert the Set Level events to Pulse events
				for (TimeSpan currentTime = startTime; currentTime < endTime; currentTime += TimeWindowSize)
				{
					endTimeWindow = currentTime + TimeWindowSize;

					// get all of the set level events in this time window
					List<EffectNode> setLevelEffectsInTimeWindow = setLevelEffects.Where(x => (x.StartTime <= currentTime) && (x.EndTime >= endTimeWindow)).ToList();

					// Logging.Info("TranslateEffects: currentTime: '" + currentTime + "' endTimeWindow: '" + endTimeWindow + "' setLevelEffectsInTimeWindow '" + setLevelEffectsInTimeWindow.Count + "'");

					Color currentColor = Color.FromArgb(0, 0, 0, 0);

					double red = 0;
					double green = 0;
					double blue = 0;

					// combine the colors
					foreach (EffectNode currentEffect in setLevelEffectsInTimeWindow)
					{
						// get the color and intensity at this point for this effect
						Color effectColor = (Color)currentEffect.Effect.ParameterValues[PULSE_COLOR_PARAMETER_INDEX];
						double effectIntensity = (double)currentEffect.Effect.ParameterValues[SET_LEVEL_INTENSITY_PARAMETER_INDEX];

						red += effectColor.R * effectIntensity;
						green += effectColor.G * effectIntensity;
						blue += effectColor.B * effectIntensity;
					} // end accumulate the color values at this time slice

					// get the largest intensity value
					double maxIntensity = Math.Max(Math.Max(red, green), blue);
					if (0 < maxIntensity)
					{
						double intensityDivisor = maxIntensity / Byte.MaxValue;

						// adjust the color valuse so the max value is 255
						red /= intensityDivisor;
						green /= intensityDivisor;
						blue /= intensityDivisor;
					} // end we have a non zero intensity

					// get the aggregate intensity
					double currentIntensity = red + blue + green;

					// create the resulting color
					currentColor = Color.FromArgb(255, Convert.ToInt32(red), Convert.ToInt32(green), Convert.ToInt32(blue));

					// Logging.Info("TranslateEffects: currentIntensity: '" + currentIntensity + "' lastIntensity: '" + lastIntensity + "' currentColor: '" + currentColor + "' lastColor: '" + lastColor + "'");

					// have we hit a dead spot?
					if (0 == currentIntensity)
					{
						// do we have any points to convert?
						if (0 != listOfPoints.Count)
						{
							// create the new pulse effect based on the current list of points
							if (null != (pulseEffectNode = CreatePulseEffect(pulseStartTime, currentTime, listOfPoints, vixElement)))
							{
								listOfEffects.Add(pulseEffectNode);
							}

							// clean up the list and prepare for the next pulse effect
							listOfPoints.Clear();
							lastColor = Color.FromArgb(0, 0, 0, 0);
							lastIntensity = 0.0;
						} // end convert the points

						// update the next pulse start time
						pulseStartTime = endTimeWindow;

						continue;
					} // end handle dead spot

					// we now have a color and intensity for this point in the curve. Has it changed?
					if ((m_minIntensityChange > Math.Abs(currentIntensity - lastIntensity)) && (currentColor == lastColor))
					{
						// no change we are interested in. Move on
						continue;
					} // end detect change

					if (0 != listOfPoints.Count)
					{
						// close the previous color
						listOfPoints.Add(Convert.ToDouble(lastColor.ToArgb()), 100.0, Convert.ToDouble(currentTime.TotalMilliseconds) - 1);
					}

					// we have a significant change. Add an entry to the list of changes
					// Logging.Info("TranslateEffects: Adding point: currentIntensity: '" + currentIntensity + "' lastIntensity: '" + lastIntensity + "' currentColor: '" + currentColor + "' lastColor: '" + lastColor + "'");
					listOfPoints.Add(Convert.ToDouble(currentColor.ToArgb()), 100.0, Convert.ToDouble(currentTime.TotalMilliseconds));
					lastColor = currentColor;
					lastIntensity = currentIntensity;
				} // end convert to pulse

				// all of the effects have been processed,
				// Do we need to close the last effect?
				if (0 != listOfPoints.Count)
				{
					// create the new pulse effect based on the current list of points
					if (null != (pulseEffectNode = CreatePulseEffect(pulseStartTime, endTime, listOfPoints, vixElement)))
					{
						listOfEffects.Add(pulseEffectNode);
					}

				} // end close last effect

			} while (false);

			return listOfEffects;
		} // TranslateSetLevelEffects

		/// <summary>
		/// Create a pulse effect for the list of points found in the list of points
		/// </summary>
		/// <param name="pulseStartTime"></param>
		/// <param name="endTimeWindow"></param>
		/// <param name="pointsInCurrentEffect"></param>
		/// <param name="vixElement"></param>
		/// <returns>pulse effect</returns>
		private EffectNode CreatePulseEffect(TimeSpan pulseStartTime, TimeSpan pulseEndTime, PointPairList pointsInCurrentEffect, ElementNode vixElement)
		{
			EffectNode newEffectNode = null;

			// Logging.Info("CreatePulseEffect: pulseStartTime: '" + pulseStartTime + "' pulseEndTime: '" + pulseEndTime + "' pointsInCurrentEffect: '" + pointsInCurrentEffect.Count + "'");

			do
			{
				// allocate a pulse effect Module
				IEffectModuleInstance pulseInstance = ApplicationServices.Get<IEffectModuleInstance>(new PulseDescriptor().TypeId);
				if (null == pulseInstance)
				{
					Logging.Error("CreatePulseEffect: Could not allocate an instance of IEffectModuleInstance");
					break;
				} // end could not allocate a pulse instance

				// Clone() Doesn't work! :(
				pulseInstance.TargetNodes = new ElementNode[] { vixElement };
				pulseInstance.TimeSpan = (pulseEndTime - pulseStartTime).Duration();

				if (null == (newEffectNode = new EffectNode(pulseInstance, pulseStartTime)))
				{
					// could not allocate the structure
					Logging.Error("CreatePulseEffect: Could not allocate an instance of EffectNode");
					break;
				} // end could not allocate an effect instance

				PointPairList pointPairList = new PointPairList();
				Curve effectCurve = new Curve(pointPairList);
				effectCurve.Points.Clear();
				// effectCurve.Points.Add(new PointPair(0.0, 100.0));
				// effectCurve.Points.Add(new PointPair(100.0, 100.0));

				// create a list of color points for the gradient
				ColorGradient colorGradient = new ColorGradient();
				colorGradient.Colors.Clear();

				// process each point in the list
				foreach (PointPair currentPoint in pointsInCurrentEffect)
				{
					// calculate the offset of the point as a ratio of the entire effect length
					double pointOffsetInPercent = ((currentPoint.Z - pulseStartTime.TotalMilliseconds) / pulseInstance.TimeSpan.TotalMilliseconds) * 100.0;

					// add the point to the intensity list
					effectCurve.Points.Add(new PointPair(pointOffsetInPercent, currentPoint.Y));

					// if the color has changed, add a color to the color list. All color changes are instant (no fading in or out)
					colorGradient.Colors.Add(new ColorPoint(Color.FromArgb(Convert.ToInt32(currentPoint.X)), (pointOffsetInPercent / 100.0)));

					// Logging.Info("CreatePulseEffect: pointTime: '" + currentPoint.Z + "' pointOffsetInPercent: '" + pointOffsetInPercent + "' color: '" + Color.FromArgb(Convert.ToInt32(currentPoint.X)) + "' intensity: '" + currentPoint.Y + "'");
				} // end process each point

				// we must have a closing point
				effectCurve.Points.Add(new PointPair(100.0, pointsInCurrentEffect.Last().Y));

				// fill in the pusle parameters
				newEffectNode.Effect.ParameterValues = new Object[]
					{
						// create the parameter values
						new Curve(effectCurve.Points), new ColorGradient(colorGradient)
					};

			} while (false);

			return newEffectNode;
		} // CreatePulseEffect
	} // LorRgbChannel
} // VixenModules.SequenceType.LightOrama