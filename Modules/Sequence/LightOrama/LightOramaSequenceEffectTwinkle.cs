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
	/// <summary>
	/// Twinkle effect
	/// </summary>
	public class LorEffectTwinkle : LorBaseEffect, ILorEffect
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public LorEffectTwinkle(XElement element) : base(element) { }
		public LorEffectTwinkle() { }

		/// <summary>
		/// Translate an effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
 		/// <param name="forcePulseEffect"></param>
		/// <returns></returns>
		public EffectNode TranslateEffect(ElementNode element, Color color, bool forcePulseEffect)
		{
			EffectNode response = null;
			// min time interval in ms
			const UInt64 MIN_TIME_INTERVAL = 50;
			const UInt64 TWINKLEPERIOD = 400;

			// make sure the intensity is valid.
			if (0 == Intensity && 0 == StartIntensity && 0 == EndIntensity)
			{
				Intensity = Byte.MaxValue;
			}

			Random random = new Random(1);
			UInt64 twinkleState = Convert.ToUInt64(Convert.ToDouble(random.Next(200)) / 100.0) & 0x01;
			UInt64 effectLenMs = (EndTimeMs - StartTimeMs);
			UInt64 nextTwinkle = Convert.ToUInt64(Convert.ToDouble(random.Next()) * TWINKLEPERIOD + 100) / MIN_TIME_INTERVAL;

			if (Intensity > 0)
			{
				for (UInt64 i = 0; i < effectLenMs; i += MIN_TIME_INTERVAL)
				{
					// add some randomness to the color handling
					byte red = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.R) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					byte green = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.G) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					byte blue = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.B) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					color = Color.FromArgb(255, red, green, blue);

					// response.Add(GenerateSetLevelEffect(element, color, effectLenMs + StartTimeMs, TWINKLEPERIOD, Intensity));

					nextTwinkle--;
					if (nextTwinkle <= 0)
					{
						twinkleState = 1 - twinkleState;
						nextTwinkle = Convert.ToUInt64(Convert.ToDouble(random.Next(1)) * TWINKLEPERIOD + 100) / MIN_TIME_INTERVAL;
					}
				}
			} // constant level
			else if (StartIntensity > 0 || EndIntensity > 0)
			{
				// ramp
				double rampdiff = Convert.ToDouble(EndIntensity - StartIntensity);
				for (UInt64 i = 0; i < effectLenMs; i += MIN_TIME_INTERVAL)
				{
					Intensity = Convert.ToUInt64(Convert.ToDouble(i) / effectLenMs * rampdiff + Convert.ToDouble(StartIntensity));

					// add some randomness to the color handling
					byte red = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.R) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					byte green = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.G) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					byte blue = Convert.ToByte(Math.Min(Byte.MaxValue, (Convert.ToUInt64(color.B) * Convert.ToUInt64(random.Next(Byte.MaxValue)) * Intensity) / (Byte.MaxValue * Byte.MaxValue)));
					color = Color.FromArgb(255, red, green, blue);

					// response.Add(GenerateSetLevelEffect(element, color, i + StartTimeMs, TWINKLEPERIOD, Intensity));

					nextTwinkle--;
					if (nextTwinkle <= 0)
					{
						twinkleState = 1 - twinkleState;
						nextTwinkle = Convert.ToUInt64(Convert.ToDouble(random.Next(1)) * TWINKLEPERIOD + 100) / MIN_TIME_INTERVAL;
					}
				} // end for each twinkle eriod
			} // end ramp

			return response;
		} // translateEffect

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
		protected EffectNode GenerateSetLevelEffect(ElementNode targetNode,
													Color v3color,
													UInt64 startTime,
													UInt64 durration,
													UInt64 intensity)
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
				setLevelInstance.TimeSpan = TimeSpan.FromMilliseconds(durration + 1);

				// set the event and event starting time
				if (null == (effectNode = new EffectNode(setLevelInstance, TimeSpan.FromMilliseconds(startTime))))
				{
					// could not allocate the structure
					Logging.Error("Light-O-Rama import: Could not allocate an instance of EffectNode");
					break;
				}

				// set intensity and color
				effectNode.Effect.ParameterValues = new object[] { (Convert.ToDouble(intensity) / Convert.ToDouble(byte.MaxValue)), v3color };
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

				Logging.Error("LOR translateEffect. Unsupported effect type 'twinkle'. Ignoring effect");

			} while (false);

			return response;
		} // getIntensityAtTime	
	} // LorEffectTwinkle
} // VixenModules.SequenceType.LightOrama