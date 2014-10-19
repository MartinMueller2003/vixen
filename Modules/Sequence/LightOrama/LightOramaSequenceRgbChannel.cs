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
	public class LorRgbChannel : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; private set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public List<ILorEffect> Effects { get; private set; }

		private Dictionary<UInt64, ILorObject> m_sequenceObjects = null;

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorRgbChannel(XElement element, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			Children = new List<UInt64>();
			Parents = new List<UInt64>();
			Name = string.Empty;
			Index = 0;
			TotalMs = 0;
			m_sequenceObjects = sequenceObjects;
			Parse(element);
		} // LorRgbChannel

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="channel"></param>
		public void Parse(XElement channel)
		{
			Index = (channel.Attribute("savedIndex") == null) ? Index : UInt64.Parse(channel.Attribute("savedIndex").Value);
			TotalMs = (channel.Attribute("totalCentiseconds") == null) ? Index : (UInt64.Parse(channel.Attribute("totalCentiseconds").Value) * 10);
			Name = (channel.Attribute("name") == null) ? Name : channel.Attribute("name").Value;

			foreach (XElement element in channel.Elements().ToList())
			{
				switch (element.Name.ToString())
				{
					case "channels":
						foreach (XElement rgbChannel in element.Elements("channel").ToList())
						{
							UInt64 childIndex = (null == rgbChannel.Attribute("savedIndex")) ? UInt64.MaxValue : UInt64.Parse(rgbChannel.Attribute("savedIndex").Value);
							Children.Add(childIndex);
							// mark the child as having a parent
							m_sequenceObjects[childIndex].Parents.Add(Index);
						}
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...rgbChannel.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse

		/// <summary>
		/// Examine the effects on the color mixing channels and consolidate them.
		/// </summary>
		public void ConsolidateEffects(ISequence Sequence, ElementNode vixElement)
		{
			// process the effects for each child
			foreach (UInt64 childIndex in Children)
			{
				LorChannel child = m_sequenceObjects[childIndex] as LorChannel;
				foreach (ILorEffect sampleEffect in child.Effects.Where(x => (false == x.HasBeenProcessed) && (x.StartTimeMs < x.EndTimeMs)))
				{
					List<ILorEffect> effectList = new List<ILorEffect>();

					// process this effect for each child
					foreach (UInt64 effectChildIndex in Children)
					{
						LorChannel effectChild = m_sequenceObjects[effectChildIndex] as LorChannel;
						// find peer effects
						foreach (ILorEffect effect in effectChild.Effects.Where(x => (false == x.HasBeenProcessed) && (x.StartTimeMs == sampleEffect.StartTimeMs) && (x.EndTimeMs == sampleEffect.EndTimeMs) && (x.RampUp == sampleEffect.RampUp) && (x.RampDown == sampleEffect.RampDown) && (x.GetType() == sampleEffect.GetType())))
						{
							// add the color to the effect for mixing
							effect.Color = effectChild.Color;
							effectList.Add(effect);
						} // end collect peer effects
					} // end collect effect from a child

					ILorEffect finalEffect = effectList.First().CombineEffects(effectList);
					Sequence.InsertData(finalEffect.translateEffect(vixElement, finalEffect.Color));
				} // end main list of effects
			} // end for each child 
		} // ConsolidateEffects
	} // LorRgbChannel
} // VixenModules.SequenceType.LightOrama