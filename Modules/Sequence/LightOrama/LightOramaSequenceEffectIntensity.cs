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

		public LorEffectIntensity(XElement element) : base(element) { }

		public EffectNode translateEffect(ElementNode element, Color color)
		{
			EffectNode node = null;
			// is this a constant pulse?
			if (0 < Intensity)
			{
				node = GenerateSetLevelEffect(element, color);
			}
			else if (EndIntensity == StartIntensity)
			{
				Intensity = StartIntensity;
				node = GenerateSetLevelEffect(element, color);
			}
			// ramp
			else
			{
				node = GeneratePulseEffect(element, color);
			}
			return node;
		} // translateEffect
	} // LorEffectIntensity
} // VixenModules.SequenceType.LightOrama