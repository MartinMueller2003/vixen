﻿using NLog;
using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Vixen.Data.Flow;
using Vixen.Module.Effect;
using Vixen.Module.OutputFilter;
using Vixen.Module.Property;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using VixenModules.App.Curves;
using VixenModules.App.ColorGradients;
using VixenModules.OutputFilter.ColorBreakdown;
using VixenModules.Property.Color;
using VixenModules.Sequence.Timed;
using ZedGraph;

namespace VixenModules.SequenceType.LightOrama
{
	public class LorRgbChannel : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; set; }
		public UInt64 Index { get; private set; }
		public UInt64 TotalMs { get; private set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public List<ILorEffect> Effects { get; private set; }
		public Guid ElementId { get; private set; }

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
			ElementId = Guid.Empty;
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

							// do we already know about this child (reparse)?
							if (0 == Children.Where(x => x == childIndex).ToList().Count)
							{
								Children.Add(childIndex);
							} // end link parent to child

							// do the child already know about this parent (reparse)?
							if (0 == m_sequenceObjects[childIndex].Parents.Where(x => x == Index).ToList().Count)
							{
								// mark the child as having a parent
								m_sequenceObjects[childIndex].Parents.Add(Index);
							} // end link child to parent
						}
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...rgbChannel.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse

		/// <summary>
		/// create this element in a tree of elements. Create any parents as needed
		/// </summary>
		/// <param name="sequence"></param>
		public void CreateVixenElement(LightOramaSequenceData sequence)
		{
			do
			{
				// do we already have an element ID
				if (Guid.Empty != ElementId)
				{
					// just go away
					break;
				} // end filter creation of element

				// create an element for this lor object
				ElementNode element = ElementNodeService.Instance.CreateSingle(null, Name);
				ElementId = element.Id;

				// Bind to the parent nodes.
				foreach (UInt64 parentId in Parents)
				{
					ILorObject parentObject = sequence.SequenceObjects[parentId];
					parentObject.CreateVixenElement(sequence);

					// bind the parent to this node
					ElementNode parentElement = VixenSystem.Nodes.GetElementNode(parentObject.ElementId);
					VixenSystem.Nodes.AddChildToParent(element, parentElement);
				} // end bind to parents

				// check to see if we should be at the root level?
				if (0 != Parents.Count)
				{
					VixenSystem.Nodes.RemoveNode(element, null, true);
				}

				// get the color handling property
				ColorModule colorProperty = element.Properties.Add(ColorDescriptor.ModuleId) as ColorModule;

				colorProperty.ColorType = ElementColorType.FullColor;
				colorProperty.SingleColor = Color.Empty;
				colorProperty.ColorSetName = String.Empty;

				ColorBreakdownModule breakdown = ApplicationServices.Get<IOutputFilterModuleInstance>(ColorBreakdownDescriptor.ModuleId) as ColorBreakdownModule;
				VixenSystem.DataFlow.SetComponentSource(breakdown, new DataFlowComponentReference(VixenSystem.DataFlow.GetComponent(element.Element.Id), 0));
				VixenSystem.Filters.AddFilter(breakdown);

				// build the color outputs
				List<ColorBreakdownItem> newBreakdownItems = new List<ColorBreakdownItem>();

				// get the color order from the children
				foreach (UInt64 childId in Children)
				{
					LorChannel child = sequence.SequenceObjects[childId] as LorChannel;
					ColorBreakdownItem cbi = new ColorBreakdownItem();
					cbi.Color = child.Color;
					cbi.Name = child.Color.Name;
					newBreakdownItems.Add(cbi);
				} // end process child colors

				breakdown.MixColors = true;
				breakdown.BreakdownItems = newBreakdownItems;
			} while (false);
		} // CreateVixenElement

		/// <summary>
		/// Map the leaf objects to Vixen elements of the same name. RGB Channel does NOT add its children to the map.
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		public int addLorObjectToMap(List<LorChannelMapping> mappings)
		{
			return AddToMappings(mappings);
		} // addLorObjectToMap

		/// <summary>
		/// Update the mappings for this channel
		/// </summary>
		/// <param name="mappings"></param>
		/// <returns>Number of channels added</returns>
		public int AddToMappings(List<LorChannelMapping> mappings)
		{
			// get the mapping for this channel
			LorChannelMapping mapping = mappings.FirstOrDefault(x => x.ChannelName == Name);
			if (null == mapping)
			{
				// this is a new mapping
				mapping = new LorChannelMapping(Name,
												Color.Empty,
												Index,
												ElementId,
												Color.Empty,
												true);
				mappings.Add(mapping);
			}
			else
			{
				// update the data in the existing mapping
				mapping.DestinationColor = Color.Empty;
				mapping.ColorMixing = true;
				mapping.ElementNodeId = ElementId;
			}

			return 1;
		} // AddToMappings

		/// <summary>
		/// Translate the effects for this channel
		/// </summary>
		/// <param name="vixElement"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public IEnumerable<EffectNode> TranslateEffects(ElementNode vixElement, System.Drawing.Color color)
		{
			List<EffectNode> listOfEffects = new List<EffectNode>();

			// get the color order from the children
			foreach (UInt64 childId in Children)
			{
				listOfEffects.AddRange(m_sequenceObjects[childId].TranslateEffects(vixElement, (m_sequenceObjects[childId] as LorChannel).Color));
			} // end process child colors

			return listOfEffects;
		} // TranslateEffects	
	} // LorRgbChannel
} // VixenModules.SequenceType.LightOrama