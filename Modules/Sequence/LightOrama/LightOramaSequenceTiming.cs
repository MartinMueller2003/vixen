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
	public class LorTiming
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		public UInt64 TimeMS { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorTiming()
		{
			TimeMS = 0;
		} // LorTiming

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="timinglElement"></param>
		public void Parse(XElement timinglElement)
		{
			TimeMS = (timinglElement.Attribute("centiseconds") == null) ? 0 : (UInt64.Parse(timinglElement.Attribute("centiseconds").Value) * 10);

			foreach (XElement element in timinglElement.Elements().ToList())
			{
				// Logging.Info("Element Name: Timings.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					default:
						Logging.Error("Skipping unsupported LOR sequence Element timings.timing.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // LorTiming

	public class LorTimings
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public Dictionary<UInt64, LorTiming> Timings { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorTimings()
		{
			Timings = new Dictionary<UInt64, LorTiming>();
		} // LorTimings

		/// <summary>
		/// Build the hierarchy that exists below the Timings node in the LOR sequence
		/// </summary>
		/// <param name="Timings"></param>
		public void Parse(XElement timings)
		{
			foreach (XElement element in timings.Elements().ToList())
			{
				// Logging.Info("Element Name: Timings.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "timing":
						LorTiming newTiming = new LorTiming();
						newTiming.Parse(element);
						Timings.Add(Convert.ToUInt64(Timings.Count), newTiming);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element timings.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // class LorTimings
} // VixenModules.SequenceType.LightOrama