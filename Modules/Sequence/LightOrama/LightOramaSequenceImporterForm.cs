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

			//I think this was the correct way to implement this.
			StaticModuleData = (LightOramaSequenceStaticData)staticModuleData;

			m_ImportFile = LightOramaFile;

			//Add known information:
			lightOramaSequenceTextBox.Text = m_ImportFile;

			//Go ahead and build the map for the sequence that we have.
			ParseSequenceData();

			//we parsed the sequence so go ahead and for now set our ChannelMappings to the parsed data
			//If the user selects one from the listbox we will make an adjustment.
			channelMappings = parsedLORSequence.mappings;

			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				LoadMaps();
			}
			else
			{
				mapExists = false;
				//use the profilename for now
				lightOramaToVixen3MappingTextBox.Text = parsedLORSequence.ProfileName;
			}
		}

		private void LoadMaps()
		{
			mapExists = true;
			//disable the convertButton
			convertButton.Enabled = false;

			PopulateListBox();
		}

		private void PopulateListBox()
		{
			lightOramaToVixen3MappingListBox.Items.Clear();

			lightOramaToVixen3MappingTextBox.Text = string.Empty;

			//iterate over the dictionary to poplulate the listbox with the mappings
			foreach (KeyValuePair<string, List<LorChannelMapping>> kvp in StaticModuleData.LightOramaMappings)
			{
				lightOramaToVixen3MappingListBox.Items.Add(kvp.Key);
			}
		}

		/// <summary>
		/// Parse the input file into something we can work with
		/// </summary>
		private void ParseSequenceData()
		{
			parsedLORSequence = new LightOramaSequenceData(m_ImportFile);

			if (!String.IsNullOrEmpty(parsedLORSequence.ProfilePath))
			{
				lightOramaProfileTextBox.Text = string.Format(@"{0}\{1}.pro", parsedLORSequence.ProfilePath, parsedLORSequence.ProfileName);
				lightOramaToVixen3MappingListBox.Text = parsedLORSequence.ProfileName;
			}
			else
			{
				lightOramaProfileTextBox.Text = parsedLORSequence.ProfileName;
			}
		}

		private void AddDictionaryEntry(string key, List<LorChannelMapping> value)
		{
			if (StaticModuleData.LightOramaMappings.ContainsKey(key))
			{
				StaticModuleData.LightOramaMappings[key] = value;
			}
			else
			{
				StaticModuleData.LightOramaMappings.Add(key, value);
			}
		}

		private void createMapButton_Click(object sender, EventArgs e)
		{
			using ( LightOramaSequenceImporterChannelMapper mappingForm =
					new LightOramaSequenceImporterChannelMapper(channelMappings, mapExists, lightOramaToVixen3MappingTextBox.Text, parsedLORSequence))
			{
				if (mappingForm.ShowDialog() == DialogResult.OK)
				{
					//add to or update the dictionary
					AddDictionaryEntry(mappingForm.MappingName, mappingForm.Mappings);

					//Clear out the text box and make the user re-select the mapping
					lightOramaToVixen3MappingTextBox.Text = string.Empty;
				}

				//User either created a new map or canceled out of the form so lets reload our
				//maps which will disable the convert button and clean out the LOR to Vixen 3 Map text box
				LoadMaps();
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Sequence = null;
		}

		private void convertButton_Click(object sender, EventArgs e)
		{
			//check to see if the mapping table is there.
			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				vixen3SequenceCreator = new Vixen3SequenceCreator(parsedLORSequence, StaticModuleData.LightOramaMappings[lightOramaToVixen3MappingTextBox.Text]);

				Sequence = vixen3SequenceCreator.Sequence;

				if (Sequence.SequenceData != null)
				{
					//we got this baby converted so close it out and load up the Sequence
					DialogResult = System.Windows.Forms.DialogResult.OK;
					Close();
				}
			}
			else
			{
				MessageBox.Show("Mapping data is missing, please try again.", "No Mapping Data", MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
			}
		}

		private void lightOramaToVixen3MappingListBox_MouseClick(object sender, MouseEventArgs e)
		{
			if (lightOramaToVixen3MappingListBox.SelectedIndex < 0 ||
				!lightOramaToVixen3MappingListBox.GetItemRectangle(lightOramaToVixen3MappingListBox.SelectedIndex).Contains(e.Location))
			{
				lightOramaToVixen3MappingListBox.SelectedIndex = -1;
				lightOramaToVixen3MappingTextBox.Text = string.Empty;

				//do not use the static mapping use the parsed sequence mapping
				//cuase the user must want to start over.
				channelMappings = parsedLORSequence.mappings;

				//disable the convert button cause we do not have a map selected
				convertButton.Enabled = false;
				createMapButton.Text = "Create New Map";
			}
			else
			{
				lightOramaToVixen3MappingTextBox.Text = lightOramaToVixen3MappingListBox.SelectedItem.ToString();

				//user selected a pre-existing mapping so use it now.
				channelMappings = StaticModuleData.LightOramaMappings[lightOramaToVixen3MappingListBox.SelectedItem.ToString()];

				//user selected a map so enable the convert button
				convertButton.Enabled = true;
				createMapButton.Text = "Edit Selected Map";
			}
		}
	}
}