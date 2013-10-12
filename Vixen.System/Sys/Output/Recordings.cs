﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Vixen.Commands;

namespace Vixen.Sys.Output
{
	class Recordings
	{
		// Stuff to work out...
		// -syncing recording state changes with async output controller operation
		// -knowing when recordings are invalid based on new hw config
		// -do we need to worry about that whole chainIndex thing?
		// -

		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		private static bool _isRecording = true;
		private static bool _isPlaying = false;
		private static bool _isMonitoring = false;

		static public bool IsRecording()
		{
			return _isRecording;
		}

		static public bool IsPlaying()
		{
			return _isPlaying;
		}

		static public bool IsMonitoring()
		{
			return _isMonitoring;
		}

		private static Stopwatch sw = Stopwatch.StartNew();
		private static long lastMs = 0;
		private static long lastSwMs = 0;

		static public void RecordStates(OutputController oc, ICommand[] states)
		{
			// anything to do?  (old school 3x faster than Linq)
			int nvals = 0;
			for( int i=0; i<states.Length; i++)
				if( states[i] != null)
					nvals++;
			if (nvals == 0)
				return;

			// find a likely context...
			Vixen.Execution.IContext ctx=null;
			foreach (var c in VixenSystem.Contexts)
			{
				if ( ! c.GetType().ToString().Contains("Sequence"))
					continue;
				ctx = c;
			}
			long thisMs = (long)ctx.GetTimeSnapshot().TotalMilliseconds;
			long deltaMs = thisMs - lastMs;

			long thisSwMs = sw.ElapsedMilliseconds;
			long deltaSwMs = thisSwMs - lastSwMs;

			Logging.Info("Name: {0}, lth:{1}, nvals:{2}, ctxms:{3}, ctxdt:{4,2}, dtsw:{5,2}, swms{6}", 
							oc.Name, states.Length, nvals, thisMs, deltaMs, deltaSwMs, thisSwMs);

			lastMs = thisMs;
			lastSwMs = thisSwMs;
		}

		static public ICommand[] GetStates(OutputController oc)
		{
			if( !_isPlaying)
				return null;

			// get the states from somewhere...
			return null;
		}

	}
}
