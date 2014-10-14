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
	// generic interface to working with LOR effects
	public interface ILorEffect
	{
		UInt64 StartTimeMs { get; }
		UInt64 EndTimeMs { get; }
		UInt64 Intensity { get; }
		UInt64 StartIntensity { get; }
		UInt64 EndIntensity { get; }

		/// <summary>
		/// translate the effect into a vixen effect and attach it to the element
		/// </summary>
		/// <param name="element"></param>
		EffectNode translateEffect(ElementNode element, Color color);

	} // ILorEffect

	/// <summary>
	/// Base storage for an LOR effect
	/// </summary>
	public class LorBaseEffect
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public UInt64 Intensity { get; set; }
		public UInt64 StartIntensity { get; private set; }
		public UInt64 EndIntensity { get; private set; }
		public UInt64 StartTimeMs { get; private set; }
		public UInt64 EndTimeMs { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorBaseEffect(XElement element)
		{
			Intensity = 0;
			StartIntensity = 0;
			EndIntensity = 0;
			StartTimeMs = 0;
			EndTimeMs = 0;
			Parse(element);
		} // LorBaseEffect

		/// <summary>
		/// Parse the xml stream
		/// </summary>
		/// <param name="effectElement"></param>
		protected void Parse(XElement effectElement)
		{
			StartTimeMs = (null == effectElement.Attribute("startCentisecond")) ? 0 : (UInt64.Parse(effectElement.Attribute("startCentisecond").Value) * 10);
			EndTimeMs = (null == effectElement.Attribute("endCentisecond")) ? 0 : (UInt64.Parse(effectElement.Attribute("endCentisecond").Value) * 10);
			Intensity = (null == effectElement.Attribute("intensity")) ? 0 : Convert.ToUInt64(Double.Parse(effectElement.Attribute("intensity").Value) * 2.55);
			StartIntensity = (null == effectElement.Attribute("startIntensity")) ? 0 : Convert.ToUInt64(Double.Parse(effectElement.Attribute("startIntensity").Value) * 2.55);
			EndIntensity = (null == effectElement.Attribute("endIntensity")) ? 0 : Convert.ToUInt64(Double.Parse(effectElement.Attribute("endIntensity").Value) * 2.55);
		} // Parse

		/// <summary>
		/// Add a constantly increasing / deacreasing ramp
		/// </summary>
		/// <param name="targetNode"></param>
		/// <returns></returns>
		protected EffectNode GeneratePulseEffect(ElementNode targetNode, Color color)
		{
			EffectNode effectNode = null;
			const double startX = 0.0;
			const double endX = 100.0;

			do
			{
				// allocate the effect Module
				IEffectModuleInstance pulseInstance = ApplicationServices.Get<IEffectModuleInstance>(Guid.Parse("cbd76d3b-c924-40ff-bad6-d1437b3dbdc0"));
				if (null == pulseInstance)
				{
					Logging.Error("GeneratePulseEffect: Could not allocate an instance of IEffectModuleInstance");
					break;
				}

				// Clone() Doesn't work! :(
				pulseInstance.TargetNodes = new ElementNode[] { targetNode };
				pulseInstance.TimeSpan = TimeSpan.FromMilliseconds((EndTimeMs - StartTimeMs) + 1);

				if (null == (effectNode = new EffectNode(pulseInstance, TimeSpan.FromMilliseconds(StartTimeMs))))
				{
					// could not allocate the structure
					Logging.Error("GeneratePulseEffect: Could not allocate an instance of EffectNode");
					break;
				}

				effectNode.Effect.ParameterValues = new Object[]
				{
					new Curve(new PointPairList(new double[] {startX, endX}, new double[] {getY(StartIntensity), getY(EndIntensity)})), new ColorGradient(color)
				};
			} while (false);

			return effectNode;
		} // end GeneratePulseEffect

		/// <summary>
		/// Add a constant level effect to the destination channel
		/// </summary>
		/// <param name="targetNode"></param>
		/// <param name="v3color"></param>
		/// <returns></returns>
		protected EffectNode GenerateSetLevelEffect(ElementNode targetNode, Color v3color)
		{
			EffectNode effectNode = null;
			IEffectModuleInstance setLevelInstance = null;

			do
			{
				if (null == (setLevelInstance = ApplicationServices.Get<IEffectModuleInstance>(Guid.Parse("32cff8e0-5b10-4466-a093-0d232c55aac0"))))
				{
					// could not get the structure
					Logging.Error("Light-O-Rama import: Could not allocate an instance of IEffectModuleInstance");
					break;
				}

				// Clone() Doesn't work! :(
				setLevelInstance.TargetNodes = new ElementNode[] { targetNode };

				// calculate how long the event lasts
				setLevelInstance.TimeSpan = TimeSpan.FromMilliseconds((EndTimeMs - StartTimeMs) + 1);

				// set the event and event starting time
				if (null == (effectNode = new EffectNode(setLevelInstance, TimeSpan.FromMilliseconds(StartTimeMs))))
				{
					// could not allocate the structure
					Logging.Error("Light-O-Rama import: Could not allocate an instance of EffectNode");
					break;
				} 

				// set intensity and color
				effectNode.Effect.ParameterValues = new object[] { (Convert.ToDouble(Intensity) / Convert.ToDouble(byte.MaxValue)), v3color };
			} while (false);

			return effectNode;
		} // end GenerateSetLevelEffect

		/// <summary>
		/// Calculate a location on the dimming curve
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected double getY(UInt64 value)
		{
			const double curveDivisor = byte.MaxValue / 100.0;

			return Convert.ToDouble(value) / curveDivisor;
		} // end getY
	} // LorBaseEffect
} // VixenModules.SequenceType.LightOrama