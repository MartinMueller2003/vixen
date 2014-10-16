using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Vixen.Sys;
using System.ComponentModel;
using Common.Controls;

namespace VixenModules.SequenceType.LightOrama
{
	public partial class LightOramaSequenceImporterChannelMapper : Form
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		private MultiSelectTreeview treeview;
		private bool MapExists;
		private int startingIndex;
		private LightOramaSequenceData m_parsedLorSequence = null;

		/// <summary>
		/// Mapping of Column headers to make code maintinance easier
		/// </summary>
		private enum mapperColumnId
		{
			lorChannelId = 0,
			lorChannelName,
			lorchannelColor,
			v3Destination,
			importColor,
			colorMixing
		};

		/// <summary>
		/// Fixed Channel offset to RGB Pixel color translation
		/// </summary>
		private Dictionary<string, List<Color>> m_defaultPixelColors = new Dictionary<string, List<Color>>()
				{
					{ "RGB", new List<Color>{ Color.Red, Color.Green, Color.Blue} },
					{ "RBG", new List<Color>{ Color.Red, Color.Blue, Color.Green} },
					{ "BRG", new List<Color>{ Color.Blue, Color.Red, Color.Green} },
					{ "BGR", new List<Color>{ Color.Blue, Color.Green, Color.Red} },
					{ "GRB", new List<Color>{ Color.Green, Color.Red, Color.Blue} },
					{ "GBR", new List<Color>{ Color.Green, Color.Blue, Color.Red} }
				};

		public List<LorChannelMapping> Mappings { get; set; }
		public string MappingName { get; set; }

		public LightOramaSequenceImporterChannelMapper(List<LorChannelMapping> mappings, bool mapExists, string mappingName, LightOramaSequenceData parsedLorSequence)
		{
			InitializeComponent();

			Mappings = mappings;
			MapExists = mapExists;
			TextBoxMappingName.Text = mappingName;
			m_parsedLorSequence = parsedLorSequence;

			checkBoxRGB.Enabled = true;
			comboBoxColorOrder.SelectedIndex = 0;
			comboBoxColorOrder.Enabled = false;
		} // LightOramaSequenceImporterChannelMapper

		/// <summary>
		/// Populate the element node multiselect tree
		/// </summary>
		private void PopulateNodeTreeMultiSelect()
		{
			multiSelectTreeview1.BeginUpdate();
			multiSelectTreeview1.Nodes.Clear();

			foreach (ElementNode element in VixenSystem.Nodes.GetRootNodes())
			{
				AddNodeToTree(multiSelectTreeview1.Nodes, element);
			}

			multiSelectTreeview1.EndUpdate();
		} // PopulateNodeTreeMultiSelect

		/// <summary>
		/// Add a node to the element tree
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="elementNode"></param>
		private void AddNodeToTree(TreeNodeCollection collection, ElementNode elementNode)
		{
			TreeNode addedNode = new TreeNode()
									{
										Name = elementNode.Id.ToString(),
										Text = elementNode.Name,
										Tag = elementNode
									};

			collection.Add(addedNode);

			foreach (ElementNode childNode in elementNode.Children)
			{
				AddNodeToTree(addedNode.Nodes, childNode);
			}
		} // AddNodeToTree

		/// <summary>
		/// Parse the mapping selection and set up the final mappings
		/// </summary>
		/// <param name="node"></param>
		private void AddVixen3ElementTolightOramaChannel(TreeNode node)
		{
			// this is a bit of a dodgy hack to allow elements to be repeated when dragged to the grid.
			// we just loop until we've repeated each element X times, in order.
			int repeatCount = decimal.ToInt32(numericUpDownRepeatElements.Value);
			if (repeatCount <= 0)
			{
				repeatCount = 1;
			}

			// Logging.Info("AddVixen3ElementTolightOramaChannel: repeatCount = " + repeatCount + ". checkBoxRGB.Checked = " + checkBoxRGB.Checked + " listViewMapping.Items.Count = " + listViewMapping.Items.Count);

			for (int i = 0; i < repeatCount; i++)
			{
				// if the user drags a large number of items to start at a position that
				// doesn't have enough 'room' off the end for them all, this can go OOR
				if (listViewMapping.Items.Count <= startingIndex)
				{
					Logging.Error("AddVixen3ElementTolightOramaChannel: Aborting because startingIndex " + startingIndex + " is greater than (or equal to) listViewMapping.Items.Count " + listViewMapping.Items.Count);
					break;
				}

				ElementNode enode = (ElementNode)node.Tag;
				ListViewItem item = listViewMapping.Items[startingIndex];

				item.SubItems[(int)mapperColumnId.v3Destination].Text = enode.Element.Name;

				item.SubItems[(int)mapperColumnId.v3Destination].Tag = enode;

				// are we processing an RGB Pixel?
				if (true == checkBoxRGB.Checked)
				{
					// use a fixed translation
					// MessageBox.Show("m_defaultPixelColors.Count: '" + m_defaultPixelColors.Count + "' comboBoxColorOrder.SelectedText: '" + comboBoxColorOrder.SelectedItem.ToString() + "' i: " + i + "'", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);					
					item.SubItems[(int)mapperColumnId.importColor].Text = m_defaultPixelColors[comboBoxColorOrder.SelectedItem.ToString()][i].Name;
					item.SubItems[(int)mapperColumnId.importColor].BackColor = m_defaultPixelColors[comboBoxColorOrder.SelectedItem.ToString()][i];
					item.SubItems[(int)mapperColumnId.colorMixing].Text = "Yes";
				} // end process pixel
				else
				{
					//Not sure where to get a node color from Vixen 3 stuff so if we have one in LOR just use it
					item.SubItems[(int)mapperColumnId.importColor].Text = item.SubItems[(int)mapperColumnId.lorchannelColor].Text;
					item.SubItems[(int)mapperColumnId.importColor].BackColor = item.SubItems[(int)mapperColumnId.lorchannelColor].BackColor;
					item.SubItems[(int)mapperColumnId.colorMixing].Text = string.Empty;
				} // end not a pixel

				startingIndex++;
			} // for each repeat element
		} // end AddVixen3ElementTolightOramaChannel

		private void ParseNodes(List<TreeNode> nodes)
		{
			foreach (TreeNode node in nodes)
			{
				//if we have node nodes assocatied with the node then this is a child node
				//so lets get our information and add it.
				if (node.Nodes.Count == 0)
				{
					//We have a node with no children so let's add it to our listviewMapping
					AddVixen3ElementTolightOramaChannel(node);
				}
				else
				{
					//lets parse it till we get to the child node
					ParseNode(node);
				}
			}
		} // ParseNodes

		private void ParseNode(TreeNode node)
		{
			foreach (TreeNode tn in node.Nodes)
			{
				if (tn.Nodes.Count != 0)
				{
					ParseNode(tn);
				}
				else
				{
					AddVixen3ElementTolightOramaChannel(tn);
				}
			}
		}

		/// <summary>
		/// Try to associate the color name with a mapped color. If no mapped color available use the system color name
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static String GetColorName(Color color)
		{
			string response = String.Empty;

			var predefined = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
			var match = (from p in predefined
						 where ((Color)p.GetValue(null, null)).ToArgb() == color.ToArgb()
						 select (Color)p.GetValue(null, null));
			if (match.Any())
			{
				response = match.First().Name;
			}
			return response;
		} // GetColorName

		/// <summary>
		/// Set up the on screen information
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LightOramaSequenceImporterChannelMapper_Load(object sender, EventArgs e)
		{
			listViewMapping.BeginUpdate();
			listViewMapping.Items.Clear();

			foreach (LorChannelMapping mapping in Mappings)
			{
				// create an empty row list
				ListViewItem item = new ListViewItem((mapping.ChannelNumber + 1).ToString()) { UseItemStyleForSubItems = false };

				// lorChannelName
				item.SubItems.Add(mapping.ChannelName);

				// lorchannelColor
				item.SubItems.Add(GetColorName(mapping.ChannelColor));

				item.SubItems[(int)mapperColumnId.lorchannelColor].BackColor = (Color)TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(GetColorName(mapping.ChannelColor));

				// do we have an existing mapping?
				if (MapExists && mapping.ElementNodeId != Guid.Empty)
				{
					// get access to the existing target node information
					ElementNode targetNode = VixenSystem.Nodes.GetElementNode(mapping.ElementNodeId);

					// v3Destination
					item.SubItems.Add(targetNode.Element.Name);
					item.SubItems[(int)mapperColumnId.v3Destination].Tag = targetNode;
				}
				else
				{
					// v3Destination
					item.SubItems.Add(string.Empty);
				}

				// importColor
				item.SubItems.Add(GetColorName(mapping.DestinationColor));
				item.SubItems[(int)mapperColumnId.importColor].BackColor = (Color)TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(GetColorName(mapping.DestinationColor));

				// colorMixing
				if (true == mapping.ColorMixing)
				{
					item.SubItems.Add("Yes");
				}
				else
				{
					item.SubItems.Add(string.Empty);
				}

				//set the lor columns to readonly
				item.SubItems[(int)mapperColumnId.lorChannelId].BackColor = Color.LightGray;
				item.SubItems[(int)mapperColumnId.lorChannelName].BackColor = Color.LightGray;

				listViewMapping.Items.Add(item);
			}

			listViewMapping.AutoResizeColumns( ColumnHeaderAutoResizeStyle.ColumnContent);
			listViewMapping.EndUpdate();

			PopulateNodeTreeMultiSelect();
		} // LightOramaSequenceImporterChannelMapper_Load

		private void CreateLorToV3MappingTable()
		{
			//default these to white
			Color lightOramaColor = Color.Empty;

			Mappings = new List<LorChannelMapping>();
			foreach (ListViewItem itemrow in listViewMapping.Items)
			{
				lightOramaColor = itemrow.SubItems[(int)mapperColumnId.lorchannelColor].BackColor;

				if (!String.IsNullOrEmpty(itemrow.SubItems[(int)mapperColumnId.v3Destination].Text))
				{
					ElementNode node = (ElementNode)itemrow.SubItems[(int)mapperColumnId.v3Destination].Tag;

					Mappings.Add(new LorChannelMapping(itemrow.SubItems[(int)mapperColumnId.lorChannelName].Text,
													lightOramaColor,
													Convert.ToUInt64(itemrow.SubItems[(int)mapperColumnId.lorChannelId].Text) - 1,
													node.Id,
													itemrow.SubItems[(int)mapperColumnId.importColor].BackColor,
													(itemrow.SubItems[(int)mapperColumnId.colorMixing].Text) == "Yes"));
				}
				else
				{
					//we are using this because we do not have a V3 map.
					Mappings.Add(new LorChannelMapping(itemrow.SubItems[(int)mapperColumnId.lorChannelName].Text,
													lightOramaColor,
													Convert.ToUInt64(itemrow.SubItems[(int)mapperColumnId.lorChannelId].Text) - 1,
													Guid.Empty,
													Color.Empty,
													false));
				}
			}

			Mappings = Mappings;
			MappingName = TextBoxMappingName.Text;
		} // CreateLorToV3MappingTable

		/// <summary>
		/// value of the RGB checkbox has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkBoxRGB_CheckedChanged(object sender, EventArgs e)
		{
			if (true == checkBoxRGB.Checked)
			{
				// disable the repeat counter and enter pixel mode
				numericUpDownRepeatElements.Enabled = false;
				numericUpDownRepeatElements.Value = 3;
				comboBoxColorOrder.Enabled = true;
			}
			else
			{
				// enable the repeat counter and exit pixel mode
				numericUpDownRepeatElements.Enabled = true;
				numericUpDownRepeatElements.Value = 1;
				comboBoxColorOrder.Enabled = false;
			}
		} // checkBoxRGB_CheckedChanged

		#region Drag drop events

		private void listViewMapping_DragDrop(object sender, DragEventArgs e)
		{
			Point cp = listViewMapping.PointToClient(new Point(e.X, e.Y));

			if (listViewMapping.HitTest(cp).Location.ToString() == "None")
			{
				//probably need to do something here
			}
			else
			{
				ListViewItem dragToItem = listViewMapping.GetItemAt(cp.X, cp.Y);

				//let the user know if we have items already here and we are about to overwrite them
				if (!String.IsNullOrEmpty(dragToItem.SubItems[(int)mapperColumnId.v3Destination].Text))
				{
					DialogResult result = MessageBox.Show("You are about to over write existing items.  Do you wish to continue?",
														  "Continue", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					if (result == System.Windows.Forms.DialogResult.OK)
					{
						startingIndex = dragToItem.Index;
						ParseNodes(treeview.SelectedNodes);
					}
				}
				else
				{
					startingIndex = dragToItem.Index;
					ParseNodes(treeview.SelectedNodes);
				}
			}
		}

		private void listViewMapping_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(TreeNode)))
				e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
		}

		private void multiSelectTreeview1_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
		}

		private void multiSelectTreeview1_ItemDrag(object sender, ItemDragEventArgs e)
		{
			treeview = (MultiSelectTreeview)sender;
			multiSelectTreeview1.DoDragDrop(e.Item, DragDropEffects.Move | DragDropEffects.Copy);
		}

		#endregion

		#region Button Events

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			//show message about cancelling
		} // buttonCancel_Click

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(TextBoxMappingName.Text))
			{
				MessageBox.Show("Please enter a name for this Map.", "Missing Name", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				CreateLorToV3MappingTable();
			}
		} // buttonOK_Click

		/// <summary>
		/// Put up a form that allows the user to choose missing elements and automatically create them
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonAutoPopulate_Click(object sender, EventArgs e)
		{
			LightOramaAutoPopulateElementsForm autoPopForm = new LightOramaAutoPopulateElementsForm(m_parsedLorSequence, Mappings);
			autoPopForm.ShowDialog();
			autoPopForm.Dispose();
			LightOramaSequenceImporterChannelMapper_Load(sender, e);
			listViewMapping.Refresh();
		} // buttonAutoPopulate_Click

		private void destinationColorButton_Click(object sender, EventArgs e)
		{
			ColorDialog colorDlg = new ColorDialog()
									{
										AllowFullOpen = true,
										AnyColor = true,
										SolidColorOnly = false,
										Color = Color.Red
									};

			if (colorDlg.ShowDialog() == DialogResult.OK)
			{
				foreach (ListViewItem itemrow in listViewMapping.SelectedItems)
				{
					itemrow.UseItemStyleForSubItems = false;
					itemrow.SubItems[(int)mapperColumnId.importColor].Text = GetColorName(colorDlg.Color);
					itemrow.SubItems[(int)mapperColumnId.importColor].BackColor = colorDlg.Color;
				}
			}
		} // destinationColorButton_Click

		#endregion

	}
}