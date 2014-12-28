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
using VixenModules.Effect.Pulse;
using VixenModules.Effect.SetLevel;
using VixenModules.Sequence.Timed;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	/// <summary>
	/// The intensity effect includes Ramp up and ramp down 
	/// </summary>
	public class LorEffectIntensity : LorBaseEffect, ILorEffect
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public LorEffectIntensity(XElement element) : base(element) { setRamps(); }
		public LorEffectIntensity() { setRamps(); }
		private void setRamps()
		{
			RampUp = EndIntensity > StartIntensity;
			RampDown = EndIntensity < StartIntensity;
		} // setRamps

		/// <summary>
		/// Translate the LOR effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <param name="forcePulseEffect"></param>
		/// <returns></returns>
		public EffectNode TranslateEffect(ElementNode element, Color color, bool forcePulseEffect)
		{
			EffectNode effectNode = null;

			do
			{
				// is this a constant pulse?
				if (0 < Intensity)
				{
					EndIntensity = StartIntensity = Intensity;
				}
				// is this a constant pulse?
				else if (EndIntensity == StartIntensity)
				{
					Intensity = StartIntensity;
				}

				setRamps();

				// is this a pulse effect?
				if (true == RampDown || true == RampUp || true == forcePulseEffect)
				{
					effectNode = TranslatePulseEffect(element, color);
				}
				else
				{
					effectNode = TranslateSetLevelEffect(element, color);
				}
			} while (false);

			return effectNode;
		} // translateEffect

		/// <summary>
		/// Translate the LOR effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		private EffectNode TranslatePulseEffect(ElementNode element, Color color)
		{
			EffectNode effectNode = null;
			const double startX = 0.0;
			const double endX = 100.0;

			do
			{
				// allocate the effect Module
				IEffectModuleInstance pulseInstance = ApplicationServices.Get<IEffectModuleInstance>(new PulseDescriptor().TypeId);
				if (null == pulseInstance)
				{
					Logging.Error("TranslatePulseEffect: Could not allocate an instance of IEffectModuleInstance");
					break;
				}

				// Clone() Doesn't work! :(
				pulseInstance.TargetNodes = new ElementNode[] { element };
				pulseInstance.TimeSpan = TimeSpan.FromMilliseconds((EndTimeMs - StartTimeMs));

				if (null == (effectNode = new EffectNode(pulseInstance, TimeSpan.FromMilliseconds(StartTimeMs))))
				{
					// could not allocate the structure
					Logging.Error("TranslatePulseEffect: Could not allocate an instance of EffectNode");
					break;
				}

				effectNode.Effect.ParameterValues = new Object[]
				{
					new Curve(new PointPairList(new double[] {startX, endX}, new double[] {getY(StartIntensity), getY(EndIntensity)})), new ColorGradient(color)
				};

			} while (false);

			return effectNode;
		} // TranslatePulseEffect

		/// <summary>
		/// Translate the LOR effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		private EffectNode TranslateSetLevelEffect(ElementNode element, Color color)
		{
			EffectNode effectNode = null;
			IEffectModuleInstance setLevelInstance = null;

			do
			{
				setLevelInstance = ApplicationServices.Get<IEffectModuleInstance>(new SetLevelDescriptor().TypeId);
				if (null == setLevelInstance)
				{
					// could not get the structure
					Logging.Error("TranslateSetLevelEffect: Could not allocate an instance of IEffectModuleInstance");
					break;
				}

				// Clone() Doesn't work! :(
				setLevelInstance.TargetNodes = new ElementNode[] { element };

				// calculate how long the event lasts
				setLevelInstance.TimeSpan = TimeSpan.FromMilliseconds(EndTimeMs - StartTimeMs);

				// set the event and event starting time
				if (null == (effectNode = new EffectNode(setLevelInstance, TimeSpan.FromMilliseconds(StartTimeMs))))
				{
					// could not allocate the structure
					Logging.Error("TranslateSetLevelEffect: Could not allocate an instance of EffectNode");
					break;
				}

				// set intensity and color
				effectNode.Effect.ParameterValues = new object[] { (Convert.ToDouble(Intensity) / Convert.ToDouble(byte.MaxValue)), color };

			} while (false);

			return effectNode;
		} // TranslateSetLevelEffect

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

		/// <summary>
		/// Get the intensity for this effect at the specified time
		/// </summary>
		/// <param name="time">Time offset in MS from start of sequence</param>
		/// <returns>The intensity at the specified time</returns>
		public UInt64 getIntensityAtTime(UInt64 time)
		{
			UInt64 response = 0;

			do
			{
				// is the desired time within our time frame?
				if ((time < StartTimeMs) || (time > EndTimeMs))
				{
					// we have nothing to contribute
					break;
				} // not for us

				// return our intensity
				// is this a constant pulse?
				if (0 < Intensity)
				{
					response = StartIntensity = Intensity;
				}
				// is this a constant pulse?
				else if (EndIntensity == StartIntensity)
				{
					response = StartIntensity;
				}
			} while (false);

			return response;
		} // getIntensityAtTime
	} // LorEffectIntensity
} // VixenModules.SequenceType.LightOrama