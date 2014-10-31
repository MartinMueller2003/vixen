using NLog;
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
	public class LorChannel : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; set; }
		public Color Color { get; private set; }
		public UInt64 Length { get; private set; }
		public UInt64 Index { get; private set; }
		public string TypeName { get { return this.GetType().ToString(); } }
		public UInt64 RgbChannel { get; set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public List<ILorEffect> Effects { get; private set; }
		public Guid ElementId { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorChannel(XElement element)
		{
			Name = string.Empty;
			Color = Color.Empty;
			Index = 0;
			Length = 0;
			RgbChannel = UInt64.MaxValue;
			Effects = new List<ILorEffect>();
			Children = new List<UInt64>();
			Parents = new List<UInt64>();
			ElementId = Guid.Empty;
			Parse(element);
		} // LorChannel

		/// <summary>
		/// Parse xml
		/// </summary>
		/// <param name="channelElement"></param>
		public void Parse(XElement channelElement)
		{
			// set the index of the channel
			Index = (null == channelElement.Attribute("savedIndex")) ? 0 : UInt64.Parse(channelElement.Attribute("savedIndex").Value);

			// fill in the attributes if they exist
			Length = (null == channelElement.Attribute("centiseconds")) ? 0 : (UInt64.Parse(channelElement.Attribute("centiseconds").Value) * 10);
			Name = (null == channelElement.Attribute("name")) ? string.Empty : channelElement.Attribute("name").Value;

			// LOR represents colors in packed BGR format. Need to convert it to ARGB
			UInt32 color = (null == channelElement.Attribute("color")) ? 0 : UInt32.Parse(channelElement.Attribute("color").Value);
			byte red = Convert.ToByte(color & 0xff);
			byte green = Convert.ToByte((color / 256) & 0xff);
			byte blue = Convert.ToByte((color / (256 * 256)) & 0xff);
			Color = Color.FromArgb(red, green, blue);

			foreach (XElement element in channelElement.Elements().ToList())
			{
				// Logging.Info("Element Name ...channels.channel.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "effect":
						ILorEffect newEffect = null;

						string effectType = (element.Attribute("type") == null) ? string.Empty : element.Attribute("type").Value.ToString();
						// what type of effect are we using?
						switch (effectType)
						{
							case "intensity":
								newEffect = new LorEffectIntensity(element);
								break;

							case "twinkle":
								newEffect = new LorEffectTwinkle(element);
								break;

							case "xshimmer":
								newEffect = new LorEffectShimmer(element);
								break;

							default:
								// give a reasonable default
								Logging.Error("Skipping unsupported LOR effect type.'" + effectType + "'");
								newEffect = new LorEffectIntensity(element);
								break;
						} // switch(effect.Attribute("type").Value)

						Effects.Add(newEffect);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element ...channels.channel.'" + element.Name.ToString() + "'");
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
				// do we already have an element ID or we are an RGB channel?
				if( (Guid.Empty != ElementId) || (UInt64.MaxValue != RgbChannel) )
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
					if (Guid.Empty == parentObject.ElementId)
					{
						parentObject.CreateVixenElement(sequence);
					} // end create parent object

					// bind the parent to this node
					ElementNode parentElement = VixenSystem.Nodes.GetElementNode(parentObject.ElementId);
					VixenSystem.Nodes.AddChildToParent(element, parentElement);
				} // end bind to parents

				// check to see if we should be at the root level?
				if (0 != Parents.Count)
				{
					VixenSystem.Nodes.RemoveNode(element, null, true);
				}

				ColorModule colorProperty = element.Properties.Add(ColorDescriptor.ModuleId) as ColorModule;
				colorProperty.ColorType = ElementColorType.SingleColor;
				colorProperty.SingleColor = Color;
				colorProperty.ColorSetName = String.Empty;

				ColorBreakdownModule breakdown = ApplicationServices.Get<IOutputFilterModuleInstance>(ColorBreakdownDescriptor.ModuleId) as ColorBreakdownModule;
				VixenSystem.DataFlow.SetComponentSource(breakdown, new DataFlowComponentReference(VixenSystem.DataFlow.GetComponent(element.Element.Id), 0));
				VixenSystem.Filters.AddFilter(breakdown);

				List<ColorBreakdownItem> newBreakdownItems = new List<ColorBreakdownItem>();

				ColorBreakdownItem cbi = new ColorBreakdownItem();
				cbi.Color = Color; ;
				cbi.Name = Color.Name;
				newBreakdownItems.Add(cbi);

				breakdown.MixColors = false;
				breakdown.BreakdownItems = newBreakdownItems;

			} while (false);
		} // CreateVixenElement
	} // LorChannel
} // VixenModules.SequenceType.LightOrama