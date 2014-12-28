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
		void CreateVixenElement(LightOramaSequenceData sequence);

		/// <summary>
		/// Map the leaf objects to Vixen elements of the same name
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		int addLorObjectToMap(List<LorChannelMapping> mappings);

		/// <summary>
		/// Update the mappings for this channel
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		int AddToMappings(List<LorChannelMapping> mappings);

		/// <summary>
		/// Translate the effects for this channel
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="color"></param>
		/// <param name="forcePulseEffect"></param>
		/// <returns>List of Cixen effects for this object</returns>
		IEnumerable<EffectNode> TranslateEffects(ElementNode vixElement, System.Drawing.Color color);

	} // ILorObject

} // VixenModules.SequenceType.LightOrama