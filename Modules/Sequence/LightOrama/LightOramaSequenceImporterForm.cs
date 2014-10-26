using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Vixen.Sys;


namespace VixenModules.SequenceType.LightOrama
{
	public partial class LightOramaSequenceImporterForm : Form
	{
		public ISequence Sequence { get; set; }

		private bool mapExists;
		private string m_ImportFile;
		private List<LorChannelMapping> channelMappings;

		private LightOramaSequenceData parsedLORSequence;
		private Vixen3SequenceCreator vixen3SequenceCreator;
		private LightOramaSequenceStaticData StaticModuleData;

		public LightOramaSequenceImporterForm(string LightOramaFile, Vixen.Module.IModuleDataModel staticModuleData)
		{
			InitializeComponent();

			channelMappings = new List<LorChannelMapping>();

			// I think this was the correct way to implement this.
			StaticModuleData = (LightOramaSequenceStaticData)staticModuleData;

			m_ImportFile = LightOramaFile;

			// Add known information:
			lightOramaSequenceTextBox.Text = m_ImportFile;

			// Go ahead and build the map for the sequence that we have.
			ParseSequenceData();

			// we parsed the sequence so go ahead and for now set our ChannelMappings to the parsed data
			// If the user selects one from the listbox we will make an adjustment.
			channelMappings = parsedLORSequence.mappings;

			// do we have any existing maps?
			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				LoadMaps();
			}
			else
			{
				mapExists = false;
			}

			// set up the channel config name. This is user entered information. It does not come from the sequence file.
			lightOramaToVixen3MappingTextBox.Text = parsedLORSequence.ChannelConfigName;
		} // LightOramaSequenceImporterForm

		/// <summary>
		/// load the conversion maps
		/// </summary>
		private void LoadMaps()
		{
			mapExists = true;

			// disable the convertButton
			convertButton.Enabled = false;
			deleteButton.Enabled = false;

			PopulateListBox();
		} // LoadMaps

		/// <summary>
		/// Populate the existing mappings into the list box.
		/// </summary>
		private void PopulateListBox()
		{
			lightOramaToVixen3MappingListBox.Items.Clear();

			lightOramaToVixen3MappingTextBox.Text = string.Empty;

			// iterate over the dictionary to poplulate the listbox with the mappings
			foreach (KeyValuePair<string, List<LorChannelMapping>> kvp in StaticModuleData.LightOramaMappings)
			{
				lightOramaToVixen3MappingListBox.Items.Add(kvp.Key);
			}
		} // PopulateListBox

		/// <summary>
		/// Parse the input file into something we can work with
		/// </summary>
		private void ParseSequenceData()
		{
			parsedLORSequence = new LightOramaSequenceData(m_ImportFile);

			lightOramaProfileTextBox.Text = parsedLORSequence.ChannelConfigName;
		} // ParseSequenceData

		/// <summary>
		/// Add an entry to the mapping table
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private void AddMappingEntry(string key, List<LorChannelMapping> value)
		{
			// add or update?
			if (StaticModuleData.LightOramaMappings.ContainsKey(key))
			{
				StaticModuleData.LightOramaMappings[key] = value;
			}
			else
			{
				StaticModuleData.LightOramaMappings.Add(key, value);
			}
		} // AddMappingEntry

		/// <summary>
		/// Process a request to create a new map
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void createMapButton_Click(object sender, EventArgs e)
		{
			using (LightOramaSequenceImporterChannelMapper mappingForm = new LightOramaSequenceImporterChannelMapper(channelMappings,
																													 mapExists,
																													 lightOramaToVixen3MappingTextBox.Text,
																													 parsedLORSequence))
			{
				// did the dialog come back with new data
				if (mappingForm.ShowDialog() == DialogResult.OK)
				{
					// add to or update the mapping entry table
					AddMappingEntry(mappingForm.MappingName, mappingForm.Mappings);

					// Clear out the text box and make the user re-select the mapping
					lightOramaToVixen3MappingTextBox.Text = string.Empty;
				} // end everything went ok

				// User either created a new map or canceled out of the form so lets reload our
				// maps which will disable the convert button and clean out the LOR to Vixen 3 Map text box
				LoadMaps();
			} // end using
		} // createMapButton_Click

		/// <summary>
		/// User decided to not continue
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			Sequence = null;
		} // cancelButton_Click

		/// <summary>
		/// Convert the current LOR sequence to a V3 sequence.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void convertButton_Click(object sender, EventArgs e)
		{
			// check to see if the mapping table is there.
			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				vixen3SequenceCreator = new Vixen3SequenceCreator(parsedLORSequence, StaticModuleData.LightOramaMappings[lightOramaToVixen3MappingTextBox.Text]);

				Sequence = vixen3SequenceCreator.Sequence;

				// do we have a resulting sequence?
				if (Sequence.SequenceData != null)
				{
					// we got this baby converted so close it out and load up the Sequence
					DialogResult = System.Windows.Forms.DialogResult.OK;
					Close();
				} // end conversion worked.
			}
			else
			{
				MessageBox.Show("Mapping data is missing, please try again.", "No Mapping Data", MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
			}
		} // convertButton_Click

		/// <summary>
		/// Delete the selected mapping
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void deleteButton_Click(object sender, EventArgs e)
		{
			// check to see if the mapping table is there.
			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				DialogResult dr =  MessageBox.Show("Are you sure you want to delete '" + lightOramaToVixen3MappingTextBox.Text + "'", 
													"Are You Sure?", 
													MessageBoxButtons.YesNo,
													MessageBoxIcon.Question);
				if( DialogResult.Yes == dr)
				{
					StaticModuleData.LightOramaMappings.Remove(lightOramaToVixen3MappingTextBox.Text);

					// rediplay the maps
					LoadMaps();
				}
			}
			else
			{
				MessageBox.Show("Mapping data is missing, please try again.", "No Mapping Data", MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
			}
		} // deleteButton_Click

		/// <summary>
		/// Activate the user selected mapping
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lightOramaToVixen3MappingListBox_MouseClick(object sender, MouseEventArgs e)
		{
			if (lightOramaToVixen3MappingListBox.SelectedIndex < 0 ||
				!lightOramaToVixen3MappingListBox.GetItemRectangle(lightOramaToVixen3MappingListBox.SelectedIndex).Contains(e.Location))
			{
				lightOramaToVixen3MappingListBox.SelectedIndex = -1;
				lightOramaToVixen3MappingTextBox.Text = string.Empty;

				// do not use the static mapping use the parsed sequence mapping
				// because the user must want to start over.
				channelMappings = parsedLORSequence.mappings;

				// disable the convert button because we do not have a map selected
				convertButton.Enabled = false;
				deleteButton.Enabled = false;
				createMapButton.Text = "Create New Map";
			}
			else
			{
				lightOramaToVixen3MappingTextBox.Text = lightOramaToVixen3MappingListBox.SelectedItem.ToString();

				// user selected a pre-existing mapping so use it now.
				channelMappings = StaticModuleData.LightOramaMappings[lightOramaToVixen3MappingListBox.SelectedItem.ToString()];

				// user selected a map so enable the convert button
				convertButton.Enabled = true;
				deleteButton.Enabled = true;
				createMapButton.Text = "Edit Selected Map";
			}
		} // lightOramaToVixen3MappingListBox_MouseClick
	} // LightOramaSequenceImporterForm
} // VixenModules.SequenceType.LightOrama