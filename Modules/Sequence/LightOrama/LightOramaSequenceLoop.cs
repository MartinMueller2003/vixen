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
	public class LorLoop
	{
		public UInt64 StartMs { get; private set; }
		public UInt64 EndMs { get; private set; }
		public UInt64 LoopCount { get; private set; }
		public string SpeedModifier { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorLoop()
		{
			StartMs = 0;
			EndMs = 0;
			LoopCount = 1;
			SpeedModifier = string.Empty;
		} // LorLoop

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="loopElement"></param>
		public void Parse(XElement loopElement)
		{
			StartMs = (loopElement.Attribute("startCentisecond") == null) ? 0 : (UInt64.Parse(loopElement.Attribute("startCentisecond").Value) * 10);
			EndMs = (loopElement.Attribute("endCentisecond") == null) ? 0 : (UInt64.Parse(loopElement.Attribute("endCentisecond").Value) * 10);
			LoopCount = (loopElement.Attribute("loopCount") == null) ? 0 : (UInt64.Parse(loopElement.Attribute("loopCount").Value));
			SpeedModifier = (loopElement.Attribute("speedModifier") == null) ? string.Empty : loopElement.Attribute("speedModifier").Value;
		} // Parse
	} // LorLoop
} // VixenModules.SequenceType.LightOrama