using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Vixen.Data.Flow;
using Vixen.Module.Effect;
using Vixen.Module.OutputFilter;
using Vixen.Module.Property;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using System.Linq;
using VixenModules.OutputFilter.ColorBreakdown;

namespace VixenModules.Property.Color
{
	public partial class ColorSetupHelper : Form, IElementSetupHelper
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		private string m_startingColorSetName = string.Empty;

		public ColorSetupHelper()
		{
			InitializeComponent();

			// RGB is the default
			comboBoxColorOrder.SelectedIndex = 0;
			comboBoxColorOrder.Enabled = false;
		}

		public string HelperName
		{
			get { return "Color Handling"; }
		}

		/// <summary>
		/// Ask the user for additional information related to setting up color handling. 
		/// </summary>
		/// <param name="selectedNodes"></param>
		/// <returns></returns>
		public bool Perform(IEnumerable<ElementNode> selectedNodes)
		{
			// This function may be called on a single node that already has a color property assigned.
			// If so, then use the existing color property values as the starting point.
			ProcessPreviousConfiguration( selectedNodes );

			DialogResult dr = ShowDialog();
			if (dr != DialogResult.OK)
				return false;

			// get access to the existing color sets
			ColorStaticData csd = ApplicationServices.GetModuleStaticData(ColorDescriptor.ModuleId) as ColorStaticData;

			// note: the color property can only be applied to leaf nodes.

			// pull out the new data settings from the form elements
			ElementColorType colorType;
			string colorSetName = "";
			int colorCount = 0;

			System.Drawing.Color singleColor = colorPanelSingleColor.Color;

			if (radioButtonOptionSingle.Checked) {
				colorType = ElementColorType.SingleColor;
				singleColor = colorPanelSingleColor.Color;
				colorCount = 1;
			}
			else if (radioButtonOptionMultiple.Checked) {
				colorType = ElementColorType.MultipleDiscreteColors;
				colorSetName = comboBoxColorSet.SelectedItem.ToString();

				// get the color set

				if (!csd.ContainsColorSet(colorSetName)) {
					Logging.Error("Color sets do not contain " + colorSetName);
				}
				else {
					colorCount = csd.GetColorSet(colorSetName).Count;
				}
			}
			else if (radioButtonOptionFullColor.Checked) {
				colorType = ElementColorType.FullColor;
				colorSetName = comboBoxColorOrder.SelectedItem.ToString();
				// name is parsed into colors.
				colorCount = colorSetName.Length;
			}
			else {
				Logging.Warn("Unexpected radio option selected");
				colorType = ElementColorType.SingleColor;
				colorCount = 1;
			}

			// PROPERTY SETUP
			// go through all elements, making a color property for each one.
			// (If any has one already, check with the user as to what they want to do.)
			IEnumerable<ElementNode> leafElements = selectedNodes.SelectMany(x => x.GetLeafEnumerator()).Distinct();
			List<ElementNode> leafElementList = leafElements.ToList();

			bool askedUserAboutExistingProperties = false;
			bool askedUserAboutOutputCountChangeInProperties = false;
			bool overrideExistingProperties = false;

			int colorPropertiesAdded = 0;
			int colorPropertiesConfigured = 0;
			int colorPropertiesSkipped = 0;

			foreach (ElementNode leafElement in leafElementList) {
				bool skip = false;
				ColorModule existingProperty = null;

				if (leafElement.Properties.Contains(ColorDescriptor.ModuleId)) {
					if (!askedUserAboutExistingProperties) {
						DialogResult mbr =
							MessageBox.Show("Some elements already have color properties set up. Should these be overwritten?",
							                "Color Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
						overrideExistingProperties = (mbr == DialogResult.Yes);
						askedUserAboutExistingProperties = true;
					}

					skip = !overrideExistingProperties;
					existingProperty = leafElement.Properties.Get(ColorDescriptor.ModuleId) as ColorModule;
				}
				else {
					existingProperty = leafElement.Properties.Add(ColorDescriptor.ModuleId) as ColorModule;
					colorPropertiesAdded++;
				}

				if (!skip) {
					if (existingProperty == null) {
						Logging.Error("Null color property for element " + leafElement.Name);
					}
					else {
						existingProperty.ColorType = colorType;
						existingProperty.SingleColor = singleColor;
						existingProperty.ColorSetName = colorSetName;
						colorPropertiesConfigured++;
					}
				}
				else {
					colorPropertiesSkipped++;
				}
			}


			// PATCHING
			// go through each element, walking the tree of patches, building up a list.  If any are a 'color
			// breakdown' already, warn/check with the user if it's OK to overwrite them.  Make a new breakdown
			// filter for each 'leaf' of the patching process. If it's fully patched to an output, recolor it or unpatch it

			List<IDataFlowComponentReference> leafOutputs = new List<IDataFlowComponentReference>();
			foreach (ElementNode leafElement in leafElementList.Where(x => x.Element != null)) {
				leafOutputs.AddRange(_FindLeafOutputsOrBreakdownFilters(VixenSystem.DataFlow.GetComponent(leafElement.Element.Id)));
			}

			bool askedUserAboutExistingFilters = false;
			bool overrideExistingFilters = false;
			ColorBreakdownModule breakdown = null;

			int colorFiltersAdded = 0;
			int colorFiltersConfigured = 0;
			int colorFiltersSkipped = 0;

			foreach (IDataFlowComponentReference leaf in leafOutputs) {
				bool skip = false;

				if (leaf.Component is ColorBreakdownModule) {
					if (!askedUserAboutExistingFilters) {
						DialogResult mbr =
							MessageBox.Show("Some elements are already patched to color filters. Should these be overwritten?",
							                "Color Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
						overrideExistingFilters = (mbr == DialogResult.Yes);
						askedUserAboutExistingFilters = true;
					}

					skip = !overrideExistingFilters;
					breakdown = leaf.Component as ColorBreakdownModule;

					// is there going to be a change in the number of patched outputs?
					if ((colorCount != breakdown.BreakdownItems.Count) && (0 != breakdown.BreakdownItems.Count))
					{
						// is there a change in the number of outputs?
						if (false == askedUserAboutOutputCountChangeInProperties)
						{
							DialogResult mbr =
								MessageBox.Show("The number of Color outputs on one or more element(s) is changing from '" + breakdown.BreakdownItems.Count + "' to '" + colorCount + "' and will be unpatched. Abort the update?",
												"Color Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
							if(mbr == DialogResult.Yes)
							{
								return false;
							}
							askedUserAboutOutputCountChangeInProperties = true;
						} // end if the user has not been asked about changes in count
					} // end there is a change in color count
				}
				else if (leaf.Component.OutputDataType == DataFlowType.None) {
					// if it's a dead-end -- ie. most likely a controller output -- skip it
					skip = true;
				}
				else {
					// doesn't exist? make a new module and assign it
					breakdown =
						ApplicationServices.Get<IOutputFilterModuleInstance>(ColorBreakdownDescriptor.ModuleId) as ColorBreakdownModule;
					VixenSystem.DataFlow.SetComponentSource(breakdown, leaf);
					VixenSystem.Filters.AddFilter(breakdown);
					colorFiltersAdded++;
				}

				if (!skip) {
					List<ColorBreakdownItem> newBreakdownItems = new List<ColorBreakdownItem>();
					bool mixColors = false;
					ColorBreakdownItem cbi;

					switch (colorType) 
					{
						case ElementColorType.FullColor:
							mixColors = true;

							// add the colors in the user requested order
							foreach (char letter in colorSetName)
							{
								// allocate a new color breakdown structure
								cbi = new ColorBreakdownItem();

								// fill it in
								switch( letter )
								{
									case 'R':
										cbi.Color = System.Drawing.Color.Red;
										break;

									case 'G':
										cbi.Color = System.Drawing.Color.Green;
										break;

									case 'B':
										cbi.Color = System.Drawing.Color.Blue;
										break;

									default:
										Logging.Error("Unexpected color in '" + comboBoxColorOrder.SelectedItem.ToString() + "'. Colors must be a combination of R, G and B");
										cbi.Color = System.Drawing.Color.Empty;
										break;
								} // end switch( letter )

								// break out the name
								cbi.Name = cbi.Color.Name;

								// save the result
								newBreakdownItems.Add(cbi);
							} // end add the colors in the desired order

							break;

						case ElementColorType.MultipleDiscreteColors:
							mixColors = false;

							if (!csd.ContainsColorSet(colorSetName)) {
								Logging.Error("Color sets doesn't contain " + colorSetName);
							}
							else {
								ColorSet cs = csd.GetColorSet(colorSetName);
								foreach (var c in cs) {
									cbi = new ColorBreakdownItem();
									cbi.Color = c;
									// heh heh, this can be.... creative.
									cbi.Name = c.Name;
									newBreakdownItems.Add(cbi);
								}
							}

							break;

						case ElementColorType.SingleColor:
							mixColors = false;
							cbi = new ColorBreakdownItem();
							cbi.Color = singleColor;
							newBreakdownItems.Add(cbi);
							break;
					} // end switch (colorType) 

					breakdown.MixColors = mixColors;

					// some special checking is needed. If the number of outputs has changed and the previous count was not zero
					// then we need to unpatch the existing outputs and replace them. If the number of outputs is unchanged, 
					// then simply update the eisting outputs with the new data and move on.
					if (newBreakdownItems.Count != breakdown.BreakdownItems.Count)
					{
						// unpatch and delete each cbi
						// get a list of the children involved in this output
						List<IDataFlowComponent> children = VixenSystem.DataFlow.GetDestinationsOfComponent(breakdown).ToList();
						foreach (IDataFlowComponent child in children)
						{
							// break the link
							VixenSystem.DataFlow.ResetComponentSource(child);
						} // end process each child

						// replace the list of items in the CBI
						breakdown.BreakdownItems = newBreakdownItems;

					} // end we have a miss match in the old and new number of color breakdown items

					// are there any existing breakdown items?
					else if (0 == breakdown.BreakdownItems.Count)
					{
						// Nope. Add the new items
						breakdown.BreakdownItems = newBreakdownItems;
					}
					else 
					{ // we have some items already defined. Just replace the color on the existing CBIs
						// for each color breakdown item in the list
						for (int index = 0; index < breakdown.BreakdownItems.Count; index++ )
						{
							// update the data for this item
							breakdown.BreakdownItems[index].Color = newBreakdownItems[index].Color;
						} // end replace the color on the CBI
					} // end same number of CBIs

					colorFiltersConfigured++;
				}
				else {
					colorFiltersSkipped++;
				}
			}

			MessageBox.Show("Color Properties:  " + colorPropertiesAdded + " added, " +
							colorPropertiesConfigured + " configured, " + colorPropertiesSkipped + " skipped. " +
			                "Color Filters:  " + colorFiltersAdded + " added, " + colorFiltersConfigured + " configured, " +
			                colorFiltersSkipped + " skipped.");

			return true;
		}

		/// <summary>
		/// Check to see if there is a color property that can be used to set the starting values. Use the last one found
		/// </summary>
		/// <param name="selectedNodes"></param>
		private void ProcessPreviousConfiguration(IEnumerable<ElementNode> selectedNodes)
		{
			IEnumerable<ElementNode> leafElements = selectedNodes.SelectMany(x => x.GetLeafEnumerator()).Distinct();
			List<ElementNode> leafElementList = leafElements.ToList();

			// process each of the properties
			foreach (ElementNode leafElement in leafElementList)
			{
				// is this a color module
				if (leafElement.Properties.Contains(ColorDescriptor.ModuleId))
				{
					// get the color module instance
					ColorModule existingProperty = leafElement.Properties.Get(ColorDescriptor.ModuleId) as ColorModule;

					// turn off all of the checkboxes
					radioButtonOptionSingle.Checked = false;
					radioButtonOptionMultiple.Checked = false;
					radioButtonOptionFullColor.Checked = false;

					// this always gets set
					colorPanelSingleColor.Color = existingProperty.SingleColor;

					// process the existing mode setting
					switch (existingProperty.ColorType)
					{
						case ElementColorType.SingleColor:
							radioButtonOptionSingle.Checked = true;
							break;

						case ElementColorType.MultipleDiscreteColors:
							m_startingColorSetName = existingProperty.ColorSetName;
							radioButtonOptionMultiple.Checked = true;
							break;

						case ElementColorType.FullColor:
							comboBoxColorOrder.SelectedItem = existingProperty.ColorSetName;
							radioButtonOptionFullColor.Checked = true;
							break;

						default:
							Logging.Error("Unsupported ElementColorType '" + existingProperty.ColorType + "'");
							break;
					} // end switch (existingProperty.ColorType)
				} // end found color property
			} // end foreach leaf node
		} // ProcessPreviousConfiguration

		private IEnumerable<IDataFlowComponentReference> _FindLeafOutputsOrBreakdownFilters(IDataFlowComponent component)
		{
			if (component == null) {
				yield break;
			}

			if (component is ColorBreakdownModule) {
				yield return new DataFlowComponentReference(component, -1);
					// this is a bit iffy -- -1 as a component output index -- but hey.
			}

			if (component.Outputs == null || component.OutputDataType == DataFlowType.None) {
				yield break;
			}

			for (int i = 0; i < component.Outputs.Length; i++) {
				IEnumerable<IDataFlowComponent> children = VixenSystem.DataFlow.GetDestinationsOfComponentOutput(component, i);

				if (!children.Any()) {
					yield return new DataFlowComponentReference(component, i);
				}
				else {
					foreach (IDataFlowComponent child in children) {
						foreach (IDataFlowComponentReference result in _FindLeafOutputsOrBreakdownFilters(child)) {
							yield return result;
						}
					}
				}
			}
		}



		private void ColorSetupHelper_Load(object sender, EventArgs e)
		{
			PopulateColorSetsComboBox();

		}

		private void PopulateColorSetsComboBox()
		{
			comboBoxColorSet.BeginUpdate();
			comboBoxColorSet.Items.Clear();

			foreach (string colorSetName in (ApplicationServices.GetModuleStaticData(ColorDescriptor.ModuleId) as ColorStaticData).GetColorSetNames()) {
				comboBoxColorSet.Items.Add(colorSetName);
			}

			// do we have a desired starting value?
			if (false == string.IsNullOrEmpty( m_startingColorSetName ))
			{
				// use the desired value once.
				comboBoxColorSet.SelectedIndex = comboBoxColorSet.Items.IndexOf( m_startingColorSetName );
				m_startingColorSetName = string.Empty;
			}
			else if (comboBoxColorSet.SelectedIndex < 0) 
			{
				// set up the default starting value
				comboBoxColorSet.SelectedIndex = 0;
			}

			comboBoxColorSet.EndUpdate();
		}

		private void AnyRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			colorPanelSingleColor.Enabled = radioButtonOptionSingle.Checked;
			comboBoxColorSet.Enabled = radioButtonOptionMultiple.Checked;
			buttonColorSetsSetup.Enabled = radioButtonOptionMultiple.Checked;
			comboBoxColorOrder.Enabled = radioButtonOptionFullColor.Checked;

			buttonOk.Enabled = radioButtonOptionSingle.Checked || radioButtonOptionMultiple.Checked || radioButtonOptionFullColor.Checked;
		}

		private void buttonColorSetsSetup_Click(object sender, EventArgs e)
		{
			using (ColorSetsSetupForm cssf = new ColorSetsSetupForm(ApplicationServices.GetModuleStaticData(ColorDescriptor.ModuleId) as ColorStaticData)) {
				cssf.ShowDialog();
				PopulateColorSetsComboBox();
			}
		}


	}
}
