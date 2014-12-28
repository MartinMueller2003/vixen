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
		/// <param name="forcePulseEffect"></param>
		/// <returns></returns>
		public EffectNode TranslateEffect(ElementNode element, Color color, bool forcePulseEffect)
		{
			Logging.Error("LOR translateEffect. Unsupported effect type 'shimmer'. Ignoring effect");
			return null;
		} // translateEffect

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

				Logging.Error("LOR translateEffect. Unsupported effect type 'shimmer'. Ignoring effect");
			} while (false);

			return response;
		} // getIntensityAtTime
	} // LorEffectShimmer
} // VixenModules.SequenceType.LightOrama