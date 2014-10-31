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
		Color Color { get; set; }
		bool HasBeenProcessed { get; }
		bool RampUp { get; set; }
		bool RampDown { get; set; }
		ILorEffect CombineEffects(List<ILorEffect> list);

		/// <summary>
		/// translate the effect into a vixen effect and attach it to the element
		/// </summary>
		/// <param name="element"></param>
		List<EffectNode> translateEffect(ElementNode element, Color color);

	} // ILorEffect

	/// <summary>
	/// Base storage for an LOR effect
	/// </summary>
	public class LorBaseEffect
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public UInt64 Intensity { get; set; }
		public UInt64 StartIntensity { get; set; }
		public UInt64 EndIntensity { get; set; }
		public UInt64 StartTimeMs { get; set; }
		public UInt64 EndTimeMs { get; set; }
		public Color Color { get; set; }
		public bool HasBeenProcessed { get; set; }
		public bool RampUp { get; set; }
		public bool RampDown { get; set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorBaseEffect(XElement element)
		{
			SetDefaults();
			Parse(element);
		} // LorBaseEffect

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorBaseEffect()
		{
			SetDefaults();
		} // LorBaseEffect

		/// <summary>
		/// Set up defaults
		/// </summary>
		private void SetDefaults()
		{
			Intensity = 0;
			StartIntensity = 0;
			EndIntensity = 0;
			StartTimeMs = 0;
			EndTimeMs = 0;
			Color = Color.Empty;
			HasBeenProcessed = false;
			RampUp = false;
			RampDown = false;
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
	} // LorBaseEffect
} // VixenModules.SequenceType.LightOrama