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
	public class AnimationColumn
	{
		public UInt64 Channel { get; private set; }
		public UInt64 Index { get; private set; }

		/// <summary>
		/// Set defaults
		/// </summary>
		public AnimationColumn()
		{
			Channel = 0;
			Index = 0;
		} // AnimationColumn

		/// <summary>
		/// Parse the data in the column element
		/// </summary>
		/// <param name="columnElement"></param>
		public void Parse(XElement columnElement)
		{
			Channel = (columnElement.Attribute("channel") == null) ? 0 : UInt64.Parse(columnElement.Attribute("channel").Value);
			Index = (columnElement.Attribute("index") == null) ? 0 : UInt64.Parse(columnElement.Attribute("index").Value);
		} // Parse
	} // AnimationColumn

	public class AnimationRow
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public UInt64 Index { get; private set; }

		public Dictionary<UInt64, AnimationColumn> Columns { get; private set; }

		/// <summary>
		/// Set defaults
		/// </summary>
		public AnimationRow()
		{
			Index = 0;
			Columns = new Dictionary<ulong, AnimationColumn>();
		} // AnimationRow

		/// <summary>
		/// Parse the information that arrives in the animation row
		/// </summary>
		/// <param name="rowElement"></param>
		public void Parse(XElement rowElement)
		{
			Index = (rowElement.Attribute("index") == null) ? 0 : UInt64.Parse(rowElement.Attribute("index").Value);

			foreach (XElement element in rowElement.Elements().ToList())
			{
				//				Logging.Info("Element Name: animation.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "column":
						AnimationColumn newColumn = new AnimationColumn();
						newColumn.Parse(element);
						Columns.Add(newColumn.Index, newColumn);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element animation.row'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level
		} // Parse
	} // AnimationRow

	public class LorAnimation
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public UInt64 NumRows { get; private set; }
		public UInt64 Numcolumns { get; private set; }
		public string Image { get; private set; }
		public bool HideControls { get; private set; }
		public Dictionary<UInt64, AnimationRow> Rows { get; private set; }

		/// <summary>
		/// Set up defaults
		/// </summary>
		public LorAnimation()
		{
			NumRows = 0;
			Numcolumns = 0;
			Image = string.Empty;
			HideControls = false;
			Rows = new Dictionary<UInt64, AnimationRow>();
		} // LorAnimation

		/// <summary>
		/// Build the hierarchy that exists below the animation node in the LOR sequence
		/// </summary>
		/// <param name="animation"></param>
		public void parse(XElement animation, Dictionary<UInt64, ILorObject> sequenceObjects)
		{
			NumRows = (animation.Attribute("rows") == null) ? 0 : UInt64.Parse(animation.Attribute("rows").Value);
			Numcolumns = (animation.Attribute("columns") == null) ? 0 : UInt64.Parse(animation.Attribute("columns").Value);
			Image = (animation.Attribute("image") == null) ? string.Empty : animation.Attribute("image").Value;
			HideControls = (animation.Attribute("hideControls") == null) ? false : bool.Parse(animation.Attribute("hideControls").Value);

			CoversionProgressForm ImportProgressBar = new CoversionProgressForm();
			ImportProgressBar.Show();
			ImportProgressBar.SetupProgressBar(0, animation.Elements().ToList().Count);
			ImportProgressBar.StatusLineLabel = "Importing Light-O-Rama Track";

			foreach (XElement element in animation.Elements().ToList())
			{
				ImportProgressBar.IncrementProgressBar();
				Application.DoEvents();
				// Logging.Info("Element Name: animation.'" + element.Name.ToString() + "'");

				switch (element.Name.ToString())
				{
					case "row":
						AnimationRow newRow = new AnimationRow();
						newRow.Parse(element);
						Rows.Add(newRow.Index, newRow);
						break;

					default:
						Logging.Error("Skipping unsupported LOR sequence Element animation.'" + element.Name.ToString() + "'");
						break;
				} // elementName
			} // end process each element catagory at the sequence level

			ImportProgressBar.Close();
		} // parse
	} // class LorAnimation
} // VixenModules.SequenceType.LightOrama