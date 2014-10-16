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
	public class LorChannel : ILorObject
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public string Name { get; private set; }
		public Color Color { get; private set; }
		public UInt64 Length { get; private set; }
		public UInt64 Index { get; private set; }
		public string TypeName { get { return this.GetType().ToString(); } }
		public UInt64 RgbChannel { get; set; }
		public List<UInt64> Children { get; private set; }
		public List<UInt64> Parents { get; private set; }
		public List<ILorEffect> Effects { get; private set; }

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

							case "xtwinkle":
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
	} // LorChannel
} // VixenModules.SequenceType.LightOrama