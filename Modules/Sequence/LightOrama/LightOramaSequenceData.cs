using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Vixen.Sys;
using NLog;

namespace VixenModules.SequenceType.LightOrama
{
	public class LightOramaSequenceData
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		protected internal string FileName { get; private set; }

		protected internal string ProfileName { get; private set; }

		protected internal string ProfilePath { get; private set; }

		protected internal string SongFileName { get; private set; }

		protected internal string SongPath { get; private set; }

		protected internal UInt64 SeqLengthInMills { get; private set; }

		protected internal List<LorChannelMapping> mappings { get; private set; }

		protected internal LorTracks Tracks { get; private set; }

		protected internal LorAnimation Animation { get; private set; }

		protected internal LorChannels Channels { get; private set; }

		protected internal Dictionary<UInt64, ILorObject> SequenceObjects { get; private set; }

		protected internal LightOramaSequenceData(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException("Cannot Locate " + fileName);
			}
			FileName = fileName;
			SequenceObjects = new Dictionary<ulong, ILorObject>();
			mappings = new List<LorChannelMapping>();
			Channels = new LorChannels(SequenceObjects);
			Tracks = new LorTracks();
			Animation = new LorAnimation();
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
					root = XElement.Load(FileName);
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

					foreach (XElement element in root.Elements().ToList())
					{
//						Logging.Info("Element Name: sequence.'" + element.Name.ToString() + "'");

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
						MessageBox.Show(
							string.Format("Audio File {0} is associated with this sequence, please select the location of the audio file.",
										  SongFileName), "Select Audio Location", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

		// build the mapping list from the LOR channel list
		private void CreateMappingList()
		{
			foreach (LorChannel channel in SequenceObjects.Values.OfType<LorChannel>().ToList())
			{
				LorChannelMapping mapping = new LorChannelMapping(channel.Name, channel.Color, channel.Index, string.Empty);
				mapping.DestinationColor = channel.Color;
				mapping.ColorMixing = UInt64.MaxValue != channel.RgbChannel;
				mappings.Add(mapping);
			}
		} // CreateMappingList
	} // LightOramaSequenceData
} // VixenModules.SequenceType.LightOrama