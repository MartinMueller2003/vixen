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
		} // translateEffect

		/// <summary>
		/// Translate the LOR effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public List<EffectNode> translateEffect(ElementNode element, Color color)
		{
			List<EffectNode> nodeList = new List<EffectNode>();
			// is this a constant pulse?
			if (0 < Intensity)
			{
				nodeList.Add(GenerateSetLevelEffect(element, color));
			}
			// is this a constant pulse?
			else if (EndIntensity == StartIntensity)
			{
				Intensity = StartIntensity;
				nodeList.Add(GenerateSetLevelEffect(element, color));
			}
			else
			{
				// ramp
				nodeList.Add(GeneratePulseEffect(element, color));
			}
			return nodeList;
		} // translateEffect

		/// <summary>
		/// Combine effects from multiple channels for the same time frame into a single effect
		/// </summary>
		/// <param name="effectList"></param>
		public ILorEffect CombineEffects(List<ILorEffect> effectList)
		{
			LorEffectIntensity response = new LorEffectIntensity();

			response.Color = Color.White;
			response.Intensity = 0;
			UInt64 red = 0;
			UInt64 green = 0;
			UInt64 blue = 0;

			// process the input colors bound to this output channel
			foreach (LorEffectIntensity effect in effectList)
			{
				response.Intensity = Math.Max(response.Intensity, effect.Intensity);
				response.StartIntensity = Math.Max(response.StartIntensity, effect.StartIntensity);
				response.EndIntensity = Math.Max(response.EndIntensity, effect.EndIntensity);
				response.StartTimeMs = Math.Max(response.StartTimeMs, effect.StartTimeMs);
				response.EndTimeMs = Math.Max(response.EndTimeMs, effect.EndTimeMs);
				effect.HasBeenProcessed = true;

				red = Math.Max(red, (effect.Color.R * Math.Max(effect.EndIntensity, Math.Max(effect.Intensity, effect.StartIntensity))));
				green = Math.Max(green, (effect.Color.G * Math.Max(effect.EndIntensity, Math.Max(effect.Intensity, effect.StartIntensity))));
				blue = Math.Max(blue, (effect.Color.B * Math.Max(effect.EndIntensity, Math.Max(effect.Intensity, effect.StartIntensity))));

			} // end process each LightOrama effect

			// get the max intensity for this LightOrama channel set
			UInt64 maxIntensity = Math.Max(red, Math.Max(green, blue));

			// Scale the color to full intensity and let the intensity value attenuate it.
			if (0 < maxIntensity)
			{
				double multplier = Convert.ToDouble(byte.MaxValue) / Convert.ToDouble(maxIntensity);

				// adjust the colors back down to a valid range
				red = Math.Min(((UInt64)255), Convert.ToUInt64(Convert.ToDouble(red) * multplier));
				green = Math.Min(((UInt64)255), Convert.ToUInt64(Convert.ToDouble(green) * multplier));
				blue = Math.Min(((UInt64)255), Convert.ToUInt64(Convert.ToDouble(blue) * multplier));
			} // do we have any remaining intensity?

			// set the final color
			response.Color = Color.FromArgb(Convert.ToInt32(red), Convert.ToInt32(green), Convert.ToInt32(blue));

			return response;
		} // CombineEffects

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
	} // LorEffectIntensity
} // VixenModules.SequenceType.LightOrama