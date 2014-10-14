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
	public class LorTrack
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public UInt64 TrackLengthInMills { get; private set; }
		//		public Dictionary<UInt64, LorChannel> Channels { get; private set; }
		public Dictionary<UInt64, LorTimings> Timings { get; private set; }
		public Dictionary<UInt64, LorLoopLevels> LoopLevels { get; private set; }
		public List<UInt64> Channels { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorTrack()
		{
			TrackLengthInMills = 0;
			//			Channels = new Dictionary<ulong, LorChannel>();
			Timings = new Dictionary<UInt64, LorTimings>();
			LoopLevels = new Dictionary<UInt64, LorLoopLevels>();
			Channels = new List<UInt64>();
		} // LorTrack

		/// <summary>
		/// Extract the information from the current track
		/// </summary>
		/// <param name="trackElement"></param>
		public void Parse(XElement trackElement)
		{
			// get the length from this track
			TrackLengthInMills = (trackElement.Attribute("totalCentiseconds") == null) ? 0 : UInt64.Parse(trackElement.Attribute("totalCentiseconds").Value) * ((UInt64)10);

			foreach (XElement element in trackElement.Elements().ToList())
			{
				// Logging.Info("Element Name: tracks.track.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "channels":
						foreach (XElement channelElement in element.Elements("channel"))
						{
							UInt64 index = (null == channelElement.Attribute("savedIndex")) ? 0 : UInt64.Parse(channelElement.Attribute("savedIndex").Value);
							Channels.Add(index);
						}
						break;

					case "loopLevels":
						LorLoopLevels newLoop = new LorLoopLevels();
						newLoop.Parse(element);
						LoopLevels.Add(Convert.ToUInt64(LoopLevels.Count), newLoop);
						break;

					case "timings":
						LorTimings newTiming = new LorTimings();
						newTiming.Parse(element);
						Timings.Add(Convert.ToUInt64(Timings.Count), newTiming);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence XML Element: tracks.track'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level

			Logging.Debug("Parse Track Channel Count " + Channels.Count);
			Logging.Debug("Parse Track loopLevels Count " + LoopLevels.Count);
			Logging.Debug("Parse Track Timings Count " + Timings.Count);
		} // Parse
	} // LorTrack
} // VixenModules.SequenceType.LightOrama