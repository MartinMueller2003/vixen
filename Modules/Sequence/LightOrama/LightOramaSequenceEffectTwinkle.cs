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
		/// Parse the xml stream
		/// </summary>
		/// <param name="effectElement"></param>
		public void Parse(XElement effectElement)
		{
			base.Parse(effectElement);
		} // Parse

		/// <summary>
		/// Translate an effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public EffectNode translateEffect(ElementNode element, Color color)
		{
			Logging.Error("LOR translateEffect. Unsupported effect type 'twinkle'. Ignoring effect");
			return null;
		} // translateEffect

		/// <summary>
		/// Combine effects from multiple channels for the same time frame into a single effect
		/// </summary>
		/// <param name="effectList"></param>
		public ILorEffect CombineEffects(List<ILorEffect> effectList)
		{
			LorEffectTwinkle response = new LorEffectTwinkle();
			Logging.Error("LOR CombineEffects. Unsupported effect type 'twinkle'. Ignoring effect");
			return response;
		} // CombineEffects
	} // LorEffectTwinkle
} // VixenModules.SequenceType.LightOrama