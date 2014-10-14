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

		/// <summary>
		/// Parse the xml stream
		/// </summary>
		/// <param name="effectElement"></param>
		public void Parse(XElement effectElement)
		{
			base.Parse(effectElement);
		} // Parse

		public EffectNode translateEffect(ElementNode element, Color color)
		{
			Logging.Error("LOR translateEffect. Unsupported effect type 'shimmer'. Ignoring effect");
			return null;
		}
	} // LorEffectShimmer
} // VixenModules.SequenceType.LightOrama