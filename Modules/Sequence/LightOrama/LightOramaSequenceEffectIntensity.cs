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

		public LorEffectIntensity(XElement element) : base(element) { setRamps();}
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
		public EffectNode translateEffect(ElementNode element, Color color)
		{
			EffectNode node = null;
			// is this a constant pulse?
			if (0 < Intensity)
			{
				node = GenerateSetLevelEffect(element, color);
			}
			// is this a constant pulse?
			else if (EndIntensity == StartIntensity)
			{
				Intensity = StartIntensity;
				node = GenerateSetLevelEffect(element, color);
			}
			else
			{
				// ramp
				node = GeneratePulseEffect(element, color);
			}
			return node;
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
	} // LorEffectIntensity
} // VixenModules.SequenceType.LightOrama