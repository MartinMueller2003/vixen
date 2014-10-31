using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Vixen.Data.Flow;
using Vixen.Module.OutputFilter;
using Vixen.Module.Property;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using VixenModules.OutputFilter.ColorBreakdown;
using VixenModules.Property.Color;
using NLog;

namespace VixenModules.SequenceType.LightOrama
{
	public class LightOramaSequenceData
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		protected internal string FileName { get; private set; }
		protected internal string ChannelConfigFileName { get; set; }
		protected internal string SongFileName { get; private set; }
		protected internal string SongPath { get; private set; }
		protected internal UInt64 SeqLengthInMills { get; private set; }
		protected internal List<LorChannelMapping> Mappings { get; private set; }
		protected internal LorTracks Tracks { get; private set; }
		protected internal LorAnimation Animation { get; private set; }
		protected internal LorChannels Channels { get; private set; }
		protected internal Dictionary<UInt64, ILorObject> SequenceObjects { get; private set; }

		private bool m_duplicateCheckCompleted = false;
		private bool m_duplicateCheckPassed = false;

		/// <summary>
		/// Parse the LOR sequence and set up the data structures.
		/// </summary>
		/// <param name="fileName"></param>
		protected internal LightOramaSequenceData(string fileName)
		{
			// verify the file name / path
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException("Cannot Locate " + fileName);
			}

			FileName = fileName;
			SequenceObjects = new Dictionary<ulong, ILorObject>();
			Mappings = new List<LorChannelMapping>();
			Channels = new LorChannels(SequenceObjects);
			Tracks = new LorTracks();
			Animation = new LorAnimation();
			ChannelConfigFileName = String.Empty;

			// now do the real work
			ParseFile();
		} // LightOramaSequenceData

		/// <summary>
		/// Parse the file into a format we can use
		/// </summary>
		private void ParseFile()
		{
			do
			{
				XElement root;
				XElement sequence;
				try
				{
					// try to open the file
					Logging.Info("ParseFile: Loading LOR file '" + FileName + "'");
					root = XElement.Load(FileName, LoadOptions.None);
				}
				catch (Exception ex)
				{
					Logging.Error("ParseFile: Caught exception loading file " + ex);
					// cant go on
					throw new FileNotFoundException("Cannot Open " + FileName);
				}

				try
				{
					// the root should be Sequence
					if ("sequence" != root.Name)
					{
						// index to the sequence information
						if (null == (sequence = root.Elements("sequence").SingleOrDefault()))
						{
							Logging.Error("ParseFile: Could not locate the LOR Sequence in the input file.");
							break;
						} // end failed to find sequence info
					}
					else
					{
						// set the access to the overall sequence information
						sequence = root;
					}

					// process the top level elements
					foreach (XElement element in root.Elements().ToList())
					{
						switch (element.Name.ToString())
						{
							case "channels":
								Channels.Parse(element);
								break;

							case "xtimingGrids":
								break;

							case "tracks":
								// get the overall track information
								Tracks.Parse(element);
								SeqLengthInMills = Tracks.SeqLengthInMills;
								break;

							case "animation":
								Animation.parse(element, SequenceObjects);
								break;

							default:
								Logging.Error("Skipping unsupported LOR sequence Element: sequence.'" + element.Name.ToString() + "'");
								break;
						} // elementName
					} // end process each element catagory at the sequence level

					Logging.Info("Total tracks imported: " + Tracks.Tracks.Count);
					Logging.Info("Total channels imported: " + SequenceObjects.Count);
					Logging.Info("Total RGB channels imported: " + SequenceObjects.Values.OfType<LorRgbChannel>().ToList().Count);

					CreateMappingList();

					//Someone may have decided to not use audio so we need to check for that as well.
					SongFileName = (null == root.Attribute("musicFilename")) ? string.Empty : root.Attribute("musicFilename").Value;

					if (!String.IsNullOrEmpty(SongFileName))
					{
						// MessageBox.Show( string.Format("Audio File {0} is associated with this sequence, please select the location of the audio file.", SongFileName), "Select Audio Location", MessageBoxButtons.OK, MessageBoxIcon.Information);
						var dialog = new OpenFileDialog
						{
							Multiselect = false,
							Title = string.Format("Open Light-O-Rama Audio  [{0}]", SongFileName),
							Filter = "Audio|*.mp3|All Files (*.*)|*.*",
							RestoreDirectory = true,
							InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Vixen\Audio"
						};
						using (dialog)
						{
							if (dialog.ShowDialog() == DialogResult.OK)
							{
								SongFileName = dialog.SafeFileName;
								SongPath = Path.GetDirectoryName(dialog.FileName);
							}
						}
					} // end we have a song name
				}
				catch (Exception ex)
				{
					Logging.Error("ParseFile: Caught exception " + ex.ToString());
				}
			} while (false);
		} // ParseFile

		/// <summary>
		/// build the mapping list from the LOR channel list
		/// </summary>
		private void CreateMappingList()
		{
			// clear out the old mappings
			Mappings.Clear();

			foreach (LorChannel channel in SequenceObjects.Values.OfType<LorChannel>().ToList())
			{
				LorChannelMapping mapping = new LorChannelMapping(channel.Name,
																  channel.Color,
																  channel.Index,
																  Guid.Empty,
																  channel.Color,
																  false);
				Mappings.Add(mapping);
			} // end add leaf channels
		} // CreateMappingList

		/// <summary>
		/// Parse the file into a format we can use
		/// </summary>
		public void ParseChanConfigFile()
		{
			do
			{
				XElement root;
				XElement chanConfig;
				try
				{
					// verify the file name / path
					if (!File.Exists(ChannelConfigFileName))
					{
						throw new FileNotFoundException("Cannot Locate " + ChannelConfigFileName);
					}

					// try to open the file
					Logging.Info("ParseFile: Loading LOR Channel Configuration file '" + ChannelConfigFileName + "'");
					root = XElement.Load(ChannelConfigFileName, LoadOptions.None);
				}
				catch (Exception ex)
				{
					Logging.Error("ParseFile: Caught exception loading file " + ex);
					// cant go on
					throw new FileNotFoundException("Cannot Open " + ChannelConfigFileName);
				}

				try
				{
					// the root should be channelConfig
					if ("channelConfig" != root.Name)
					{
						// index to the sequence information
						if (null == (chanConfig = root.Elements("channelConfig").SingleOrDefault()))
						{
							Logging.Error("ParseFile: Could not locate the LOR Channel Configuration in the input file.");
							break;
						} // end failed to find sequence info
					}
					else
					{
						// set the access to the overall sequence information
						chanConfig = root;
					}

					// process the top level elements
					foreach (XElement element in root.Elements().ToList())
					{
						switch (element.Name.ToString())
						{
							case "channels":
								Channels.Parse(element);
								break;

							default:
								Logging.Error("Skipping unsupported LOR channel configuration Element: channelConfiguration.'" + element.Name.ToString() + "'");
								break;
						} // elementName
					} // end process each element catagory at the sequence level

					CreateMappingList();
				}
				catch (Exception ex)
				{
					Logging.Error("ParseFile: Caught exception " + ex.ToString());
				}

			} while (false);

		} // ParseChanConfigFile

		/// <summary>
		/// Check for duplicate element names, ask user if we can resolve issue and then clean up the names by adding a suffix to the name
		/// </summary>
		/// <returns>true if no duplicates exist</returns>
		public bool CleanUpDuplicates()
		{
			m_duplicateCheckPassed = true;
			do
			{
				// clean up duplicates that might be present
				var duplicates = SequenceObjects.GroupBy(x => x.Value.Name).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
				if (0 != duplicates.Count)
				{
					// tell the user we have duplicate names that need to be resolved
					m_duplicateCheckPassed = false;

					// have we already asked about the duplicate check?
					if (true == m_duplicateCheckCompleted)
					{
						// just give the same answer as last time
						break;
					}

					// show that we have asked the user about fixing duplicates
					m_duplicateCheckCompleted = true;

					DialogResult result = MessageBox.Show("LOR Auto Populate Elements has found duplicate elements." + Environment.NewLine +
														  "Please see the logs for a list of duplicates that must be resolved prior to conversion." + Environment.NewLine +
														  "Would you like Vixen to automatically resolve the naming errors?", "WARNING: Element Naming Error(s)", MessageBoxButtons.YesNo);
					// output to the log
					foreach (var duplicate in duplicates)
					{
						foreach (var kvp in SequenceObjects.Where(x => x.Value.Name == duplicate.Key))
						{
							Logging.Error("Found Duplicate LOR element '" + duplicate.Key + "' of type '" + kvp.Value.GetType().ToString() + "'");
						}
					} // end output to the log

					// does the user want us to modify the channel names?
					if (result == System.Windows.Forms.DialogResult.No)
					{
						break;
					} // end user has decided to not proceed

					// keep looping until all of the duplicates are gone
					while (0 != duplicates.Count)
					{
						foreach (var duplicate in duplicates)
						{
							foreach (var kvp in SequenceObjects.Where(x => x.Value.Name == duplicate.Key))
							{
								// is this a leaf node
								if ((kvp.Value.GetType() == typeof(LorRgbChannel)) || (kvp.Value.GetType() == typeof(LorChannel)))
								{
									continue;
								} // end skip leaf node

								int currentCount = 0;
								string currentName = duplicate.Key + " (" + currentCount++ + ")";
								// do not modify the names of the leaf objects. They need to correspond to the names in the channel list

								while (0 != SequenceObjects.Where(x => x.Value.Name == currentName).ToList().Count)
								{
									currentName = duplicate.Key + " (" + currentCount++ + ")";
								} // end search for a unique name.

								// we now have a unique name. set the element nname
								kvp.Value.Name = currentName;

								// names have been modified. Need to rebuild the lists and start over
								break;
							} // end replace a name

							// names have been modified. Need to rebuild the lists and start over
							break;
						} // end process duplicate

						duplicates = SequenceObjects.GroupBy(x => x.Value.Name).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
					} // end while duplicates continue to exist
				} // end duplicates found

				// tell the user that all duplicate have been resolved
				m_duplicateCheckPassed = true;
			} while (false);

			return m_duplicateCheckPassed;
		} // cleanUpDuplicates

		/// <summary>
		/// Add the node and its children to the list of elements
		/// </summary>
		/// <param name="lorObject"></param>
		public void addLorObjectToElementList(ILorObject lorObject)
		{
			// create a vixen element for this lor object
			lorObject.CreateVixenElement(this);

			// process any children the node may have
			foreach (UInt64 childIndex in lorObject.Children)
			{
				addLorObjectToElementList(SequenceObjects[childIndex]);
			} // end process the children
		} // addLorObjectToElementList

		/// <summary>
		/// Map the leaf objects to Vixen elements of the same name
		/// </summary>
		/// <param name="lorObject"></param>
		public UInt64 addLorObjectToMap(ILorObject lorObject)
		{
			UInt64 response = 0;
			ElementNode element = null;

			// v3Destination
			// does this object have any children?
			if ((0 != lorObject.Children.Count) && (lorObject.GetType() != typeof(LorRgbChannel)))
			{
				// process any children the node may have
				foreach (UInt64 childIndex in lorObject.Children)
				{
					response += addLorObjectToMap(SequenceObjects[childIndex]);
				} // end process the children
			}
			// does this object exist in the Vixen Element list?
			else if (null != (element = VixenSystem.Nodes.GetAllNodes().FirstOrDefault(x => x.Name == lorObject.Name)))
			{
				// is this an RGB channel?
				if (lorObject.GetType() == typeof(LorRgbChannel))
				{
					// process the children
					foreach (UInt64 childIndex in lorObject.Children)
					{
						response++;

						LorChannel rgbChild = SequenceObjects[childIndex] as LorChannel;
						LorChannelMapping mapping = Mappings.FirstOrDefault(x => x.ChannelName == rgbChild.Name);
						if (null == mapping)
						{
							mapping = new LorChannelMapping(rgbChild.Name,
															rgbChild.Color,
															rgbChild.Index,
															element.Id,
															rgbChild.Color,
															true);
							Mappings.Add(mapping);
						}
						else
						{
							mapping.DestinationColor = rgbChild.Color;
							mapping.ColorMixing = true;
							mapping.ElementNodeId = element.Id;
						}
					} // end RGB channels
				} // end RGB leaf
				else if ((lorObject.GetType() == typeof(LorChannel) && (UInt64.MaxValue == ((LorChannel)lorObject).RgbChannel)))
				{
					response++;

					// get the mapping entry for this element
					LorChannelMapping mapping = Mappings.FirstOrDefault(x => x.ChannelName == lorObject.Name);
					if (null != mapping)
					{
						mapping.DestinationColor = (lorObject as LorChannel).Color;
						mapping.ColorMixing = false;
						mapping.ElementNodeId = element.Id;
					} // end map exists
					else
					{
						// create a mapping for this channel
						mapping = new LorChannelMapping((lorObject as LorChannel).Name,
														(lorObject as LorChannel).Color,
														(lorObject as LorChannel).Index,
														element.Id,
														(lorObject as LorChannel).Color,
														false);
						Mappings.Add(mapping);
					}
				} // end there is an element for this lor channel
			} // end matching element exists
			else
			{
				// LOR object does not exist in the V3 Element table
			}

			return response;
		} // addLorObjectToMap
	} // LightOramaSequenceData
} // VixenModules.SequenceType.LightOrama