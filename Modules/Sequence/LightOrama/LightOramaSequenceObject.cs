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

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="channelElement"></param>
		void Parse(XElement element);

		/// <summary>
		/// create this element in a tree of elements. Create any parents as needed
		/// </summary>
		/// <param name="sequence"></param>
		void CreateVixenElement(LightOramaSequenceData dataSet);

		/// <summary>
		/// Update the mappings for this channel
		/// </summary>
		/// <param name="dataSet"></param>
		/// <returns></returns>
		int AddToMappings(LightOramaSequenceData dataSet);

		/// <summary>
		/// Translate the effects for this channel
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="color"></param>
		/// <returns>List of Cixen effects for this object</returns>
		 IEnumerable<EffectNode> TranslateEffects(ElementNode vixElement, System.Drawing.Color color);

	} // ILorObject

} // VixenModules.SequenceType.LightOrama