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
	public class LorTracks
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public Dictionary<UInt64, LorTrack> Tracks { get; private set; }
		public UInt64 SeqLengthInMills { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorTracks()
		{
			Tracks = new Dictionary<UInt64, LorTrack>();
			SeqLengthInMills = 0;
		} // LorTracks

		/// <summary>
		/// Build the hierarchy that exists below the Tracks node in the LOR sequence
		/// </summary>
		/// <param name="tracks"></param>
		public void Parse(XElement tracks)
		{
			foreach (XElement element in tracks.Elements().ToList())
			{
				// Logging.Info("Element Name: tracks.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "track":
						LorTrack newTrack = new LorTrack();
						newTrack.Parse(element);
						Tracks.Add(Convert.ToUInt64(Tracks.Count), newTrack);
						SeqLengthInMills = Math.Max(SeqLengthInMills, newTrack.TrackLengthInMills);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element tracks.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // class LorTracks

} // VixenModules.SequenceType.LightOrama