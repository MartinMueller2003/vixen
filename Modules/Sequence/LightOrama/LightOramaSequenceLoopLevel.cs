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
	public class LorLoopLevel
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		public List<LorLoop> Loops { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorLoopLevel()
		{
			Loops = new List<LorLoop>();
		} // LorLoopLevel

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="loopLevelElement"></param>
		public void Parse(XElement loopLevelElement)
		{
			foreach (XElement element in loopLevelElement.Elements().ToList())
			{
				// Logging.Info("Element Name: LoopLevels.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "loop":
						LorLoop newLoop = new LorLoop();
						newLoop.Parse(element);
						Loops.Add(newLoop);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element LoopLevels.LoopLevel.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // LorLoopLevel

	public class LorLoopLevels
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public Dictionary<UInt64, LorLoopLevel> LoopLevels { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorLoopLevels()
		{
			LoopLevels = new Dictionary<UInt64, LorLoopLevel>();
		} // LorLoopLevels

		/// <summary>
		/// Build the hierarchy that exists below the LoopLevels node in the LOR sequence
		/// </summary>
		/// <param name="loopLevels"></param>
		public void Parse(XElement loopLevels)
		{
			foreach (XElement element in loopLevels.Elements().ToList())
			{
				// Logging.Info("Element Name: LoopLevels.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "LoopLevel":
						LorLoopLevel newLoopLevel = new LorLoopLevel();
						newLoopLevel.Parse(element);
						LoopLevels.Add(Convert.ToUInt64(LoopLevels.Count), newLoopLevel);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element LoopLevels.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // class LorLoopLevels
} // VixenModules.SequenceType.LightOrama