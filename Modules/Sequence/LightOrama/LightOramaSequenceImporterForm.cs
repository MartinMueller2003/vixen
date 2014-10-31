using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Vixen.Sys;


namespace VixenModules.SequenceType.LightOrama
{
	public partial class LightOramaSequenceImporterForm : Form
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ISequence Sequence { get; set; }

		//		private bool mapExists;
		private string m_ImportFile;
		private List<LorChannelMapping> m_channelMappings;
		private bool m_createNewMap = true;

		private LightOramaSequenceData parsedLORSequence;
		private Vixen3SequenceCreator vixen3SequenceCreator;
		private LightOramaSequenceStaticData StaticModuleData;

		public LightOramaSequenceImporterForm(string LightOramaFile, Vixen.Module.IModuleDataModel staticModuleData)
		{
			InitializeComponent();

			m_channelMappings = new List<LorChannelMapping>();

			// I think this was the correct way to implement this.
			StaticModuleData = (LightOramaSequenceStaticData)staticModuleData;

			m_ImportFile = LightOramaFile;

			// Add known information:
			lightOramaSequenceTextBox.Text = m_ImportFile;

			// Go ahead and build the map for the sequence that we have.
			ParseSequenceData();

			// we parsed the sequence so go ahead and for now set our ChannelMappings to the parsed data
			// If the user selects one from the listbox we will make an adjustment.
			// channelMappings = parsedLORSequence.mappings;

			// do we have any existing maps?
			if (StaticModuleData.LightOramaMappings.Count > 0)
			{
				LoadMaps();
			}

			// set up the channel config name. This is user entered information. It does not come from the sequence file.
			lightOramaToVixen3MappingTextBox.Text = parsedLORSequence.ChannelConfigFileName;
		} // LightOramaSequenceImporterForm

		/// <summary>
		/// load the conversion maps
		/// </summary>
		private void LoadMaps()
		{
			// disable the convertButton
			convertButton.Enabled = false;
			deleteButton.Enabled = false;
			m_createNewMap = true;

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

			lightOramaChannelConfigurationTextBox.Text = parsedLORSequence.ChannelConfigFileName;
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
			// are we creating a new map?
			if (true == m_createNewMap)
			{
				// see if the user wants to use a channel configuration file
				importChannelConfiguration();

				// ask the user if they want to pre-create the Vixen elements
				createElements();

				// ask the user if they want to pre-map the elements
				createMapping();

				m_channelMappings = parsedLORSequence.Mappings;
			} // end create a new map

			using (LightOramaSequenceImporterChannelMapper mappingForm = new LightOramaSequenceImporterChannelMapper(m_channelMappings,
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
		/// Find and parse the channel configuration file
		/// </summary>
		private void importChannelConfiguration()
		{
			// does this sequence have a channel configuration file defined?
			if (true == String.IsNullOrEmpty(lightOramaChannelConfigurationTextBox.Text))
			{
				// ask the user if there is a channel configuration file available
				DialogResult dr = MessageBox.Show("Do you wish to specify a Light-O-Rama Channel Configuration for this mapping?", "Channel Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (DialogResult.Yes == dr)
				{
					var dialog = new OpenFileDialog
					{
						Multiselect = false,
						Title = string.Format("Select Light-O-Rama Channel Configuration File"),
						Filter = "ChanCfg Files (*.lcc)|*.lcc|All Files (*.*)|*.*",
						RestoreDirectory = true,
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Vixen 3\Sequence"
					};
					using (dialog)
					{
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							lightOramaChannelConfigurationTextBox.Text = dialog.FileName;
						}
					}
				} // yes the user wants to specify a channel configuration file
			} // end ask the user if they want to specify a channel configuration file

			// is a channel confiig file specified?
			if (false == String.IsNullOrEmpty(lightOramaChannelConfigurationTextBox.Text))
			{
				// save the file name
				parsedLORSequence.ChannelConfigFileName = lightOramaChannelConfigurationTextBox.Text;

				// parse the file
				parsedLORSequence.ParseChanConfigFile();
			} // end parse the channel config file
		} // importChanCfg

		/// <summary>
		/// Create V3 elements for any LOR channels that do not have an existing element. Create color handling filters at the same time.
		/// </summary>
		private void createElements()
		{
			do
			{
				// ask if we should create elements
				DialogResult dr = MessageBox.Show("Would you like Vixen to automatically create Vixen elements that correspond to your LOR channels?",
													  "Create Elements?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (DialogResult.Yes != dr)
				{
					// user does not want us to create the elements
					break;
				} // end ask about creating the vixen elements

				// make sure the element names are good. LOR allows duplicate names
				if (false == parsedLORSequence.CleanUpDuplicates())
				{
					// cannot continue. Move to manual processing mode
					break;
				} // end failed to resolve the existance of duplicates in the element names

				// process all of the top level objects
				foreach (ILorObject lorObject in parsedLORSequence.SequenceObjects.Values.Where(x => x.Parents.Count == 0).ToList())
				{
					// this will add the LOR object tree to the element tree
					parsedLORSequence.addLorObjectToElementList(lorObject);
				} // end process elements
			} while (false);
		} // createElements

		/// <summary>
		/// automatically map all of the elements and LOR channels that have matching names
		/// </summary>
		private void createMapping()
		{
			int mappingCount = 0;
			do
			{
				// ask if we should map elements
				DialogResult dr = MessageBox.Show("Would you like Vixen to automatically map Vixen elements that correspond to your LOR channels?",
												  "MAP Elements?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (DialogResult.Yes != dr)
				{
					// user does not want us to create the elements
					break;
				} // end ask about creating the vixen elements

				// make sure the element names are good. LOR allows duplicate names
				if (false == parsedLORSequence.CleanUpDuplicates())
				{
					// cannot continue. Move to manual processing mode
					break;
				} // end failed to resolve the existance of duplicates in the element names

				// process all of the top level objects
				foreach (ILorObject lorObject in parsedLORSequence.SequenceObjects.Values.Where(x => x.Parents.Count == 0).ToList())
				{
					// this will add the LOR object tree to the element tree
					mappingCount += parsedLORSequence.addLorObjectToMap(lorObject);
				} // end process elements

				MessageBox.Show("LOR Auto Map has updated " + mappingCount + " elements", "Map Vixen Elements");

			} while (false);
		} // createMapping

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
				DialogResult dr = MessageBox.Show("Are you sure you want to delete '" + lightOramaToVixen3MappingTextBox.Text + "'",
													"Are You Sure?",
													MessageBoxButtons.YesNo,
													MessageBoxIcon.Question);
				if (DialogResult.Yes == dr)
				{
					StaticModuleData.LightOramaMappings.Remove(lightOramaToVixen3MappingTextBox.Text);

					// redisplay the maps
					LoadMaps();
					lightOramaToVixen3MappingListBox_MouseClick(sender, e as MouseEventArgs);
				} // end delete existing conversion.
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
				m_channelMappings = parsedLORSequence.Mappings;

				// disable the convert button because we do not have a map selected
				convertButton.Enabled = false;
				deleteButton.Enabled = false;
				m_createNewMap = true;
				createMapButton.Text = "Create New Map";
			}
			else
			{
				lightOramaToVixen3MappingTextBox.Text = lightOramaToVixen3MappingListBox.SelectedItem.ToString();

				// user selected a pre-existing mapping so use it now.
				m_channelMappings = StaticModuleData.LightOramaMappings[lightOramaToVixen3MappingListBox.SelectedItem.ToString()];

				// user selected a map so enable the convert button
				convertButton.Enabled = true;
				deleteButton.Enabled = true;
				m_createNewMap = false;
				createMapButton.Text = "Edit Selected Map";
			}
		} // lightOramaToVixen3MappingListBox_MouseClick
	} // LightOramaSequenceImporterForm
} // VixenModules.SequenceType.LightOrama