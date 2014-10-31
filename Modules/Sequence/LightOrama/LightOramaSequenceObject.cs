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
	// generic interface to working with LOR objects. Objects may hold references to other objects sorted by type etc
	public interface ILorObject
	{
		UInt64 Index { get; }
		string Name { get; set; }
		Guid ElementId { get; }
		List<UInt64> Children { get; }
		List<UInt64> Parents { get; }

		void Parse(XElement element);
		void CreateVixenElement( LightOramaSequenceData dataSet);
	} // ILorObject

} // VixenModules.SequenceType.LightOrama