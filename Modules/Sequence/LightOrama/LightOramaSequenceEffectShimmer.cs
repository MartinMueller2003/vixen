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
	/// Shimmer effect
	/// </summary>
	public class LorEffectShimmer : LorBaseEffect, ILorEffect
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public LorEffectShimmer(XElement element) : base(element) { }
		public LorEffectShimmer() { }

		/// <summary>
		/// Translate the LOR effect into a V3 effect
		/// </summary>
		/// <param name="element"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public List<EffectNode> translateEffect(ElementNode element, Color color)
		{
			Logging.Error("LOR translateEffect. Unsupported effect type 'shimmer'. Ignoring effect");
			return new List<EffectNode>(); ;
		} // translateEffect

		/// <summary>
		/// Combine effects from multiple channels for the same time frame into a single effect
		/// </summary>
		/// <param name="effectList"></param>
		public ILorEffect CombineEffects(List<ILorEffect> effectList)
		{
			LorEffectShimmer response = new LorEffectShimmer();
			Logging.Error("LOR CombineEffects. Unsupported effect type 'twinkle'. Ignoring effect");
			return response;
		} // CombineEffects
	} // LorEffectShimmer
} // VixenModules.SequenceType.LightOrama