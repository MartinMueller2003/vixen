using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vixen.Services;
using Vixen.Sys;


namespace VixenModules.SequenceType.LightOrama
{
	public partial class LightOramaAutoPopulateElementsForm : Form
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Data used to access chanel information
		/// </summary>
		private LightOramaSequenceData m_parsedLorSequence = null;

		/// <summary>
		/// Current mappings
		/// </summary>
		List<LorChannelMapping> m_mappings = new List<LorChannelMapping>();

		private UInt64 m_mappingCount = 0;

		public LightOramaAutoPopulateElementsForm(LightOramaSequenceData parsedLorSequence, List<LorChannelMapping> mappings)
		{
			InitializeComponent();
			m_parsedLorSequence = parsedLorSequence;
			m_mappings = mappings;
			PopulateLorChannelInformation();
		} // LightOramaAutoPopulateElementsForm

		/// <summary>
		///  Parse the LOR channel structure and create the channel lists.
		/// </summary>
		private void PopulateLorChannelInformation()
		{
			treeViewLorChannels.BeginUpdate();
			treeViewLorChannels.Nodes.Clear();

			List<TreeNode> topLevelObjects = new List<TreeNode>();

			// process all of the top level objects
			foreach (ILorObject lorObject in m_parsedLorSequence.SequenceObjects.Values.Where(x => x.Parents.Count == 0))
			{
				topLevelObjects.Add(addLorChannelObject(lorObject));
			}

			treeViewLorChannels.Nodes.AddRange(topLevelObjects.ToArray());
			treeViewLorChannels.EndUpdate();
		} // PopulateLorChannelInformation

		/// <summary>
		/// Build the child node tree.
		/// </summary>
		/// <param name="lorObject"></param>
		/// <returns></returns>
		private TreeNode addLorChannelObject(ILorObject parent)
		{
			TreeNode response = null;
			List<TreeNode> children = new List<TreeNode>();

			do
			{
				// RGB channels get special handling
				if (parent.GetType() == typeof(LorRgbChannel))
				{
					// this is a leaf node with children that we ignore
					response = new TreeNode(parent.Name)
					{
						Name = parent.Index.ToString(),
						Text = parent.Name
					};

					break;
				} // end RGB processing

				// process any children this object may have
				foreach (UInt64 childIndex in parent.Children)
				{
					// add the child to the list
					children.Add(addLorChannelObject(m_parsedLorSequence.SequenceObjects[childIndex]));
				} // end processing the children

				// do we have children?
				if (0 != children.Count)
				{
					// this is a parent node
					response = new TreeNode(parent.Name, children.ToArray())
					{
						Name = parent.Index.ToString(),
						Text = parent.Name
					};
				} // end parent node
				else
				{
					// this is a leaf node
					response = new TreeNode(parent.Name)
					{
						Name = parent.Index.ToString(),
						Text = parent.Name
					};
				} // end leaf node
			} while (false);

			return response;
		} // addLorChannelObject

		/// <summary>
		/// Create V3 Elements corresponding to the selected LOR channels
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonCreateElements_Click(object sender, EventArgs e)
		{
			do
			{
				var duplicates = m_parsedLorSequence.SequenceObjects.GroupBy(x => x.Value.Name).Where(g => g.Count() > 1).ToDictionary(x=>x.Key, y=>y.Count());
				if (0 != duplicates.Count)
				{
					DialogResult result = MessageBox.Show("LOR Auto Populate Elements has found duplicate elements and cannot continue. Please see the logs for a list of duplicates that must be resolved prior to conversion.", "WARNING: Conversion Naming Error", MessageBoxButtons.YesNo);
					foreach (var duplicate in duplicates)
					{
						foreach( var kvp in m_parsedLorSequence.SequenceObjects.Where(x => x.Value.Name == duplicate.Key) )
						{
							Logging.Error("Found Duplicate LOR element '" + duplicate.Key + "' of type '" +kvp.Value.GetType().ToString() + "'");
						}
					}

					// does the user want us to modify the channel names?
					if (result == System.Windows.Forms.DialogResult.No)
					{
						break;
					}

					while (0 != duplicates.Count)
					{
						foreach (var duplicate in duplicates)
						{
							foreach (var kvp in m_parsedLorSequence.SequenceObjects.Where(x => x.Value.Name == duplicate.Key))
							{
								// is this a leaf node
								if((kvp.Value.GetType() == typeof(LorRgbChannel)) || (kvp.Value.GetType() == typeof(LorChannel)))
								{
									continue;
								} // end skip leaf node

								int currentCount = 0;
								string currentName = duplicate.Key + " (" + currentCount++ + ")";
								// do not modify the names of the leaf objects. They need to correspond to the names in the channel list

								while (0 != m_parsedLorSequence.SequenceObjects.Where(x => x.Value.Name == currentName).ToList().Count)
								{
									currentName = duplicate.Key + " (" + currentCount++ + ")";
								} // end search for a unique name.

								// we now have a unique name. set the element nname
								kvp.Value.Name = currentName;

								// names have been modified. Need to rebuild the lists and start over
								break;
							}

							// names have been modified. Need to rebuild the lists and start over
							break;
						}

						duplicates = m_parsedLorSequence.SequenceObjects.GroupBy(x => x.Value.Name).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
					}

				}

				int startingElementCount = VixenSystem.Nodes.GetAllNodes().Count();

				// process the nodes that are currently selected
				foreach (TreeNode node in treeViewLorChannels.SelectedNodes)
				{
					addLorObjectToElementList(m_parsedLorSequence.SequenceObjects[Convert.ToUInt64(node.Name)]);
				} // end process selected nodes

				MessageBox.Show("LOR Auto Populate has created " + (VixenSystem.Nodes.GetAllNodes().Count() - startingElementCount) + " elements", "Create Vixen Elements");
				Logging.Info("LOR Auto Populate has created " + (VixenSystem.Nodes.GetAllNodes().Count() - startingElementCount) + " elements");
			} while (false);
		} // buttonCreateElements_Click

		/// <summary>
		/// Add the node and its children to the list of elements
		/// </summary>
		/// <param name="lorObject"></param>
		private void addLorObjectToElementList(ILorObject lorObject)
		{
			// does this object have any children?
			if ((0 != lorObject.Children.Count) && (lorObject.GetType() != typeof(LorRgbChannel)))
			{
				// process any children the node may have
				foreach (UInt64 childIndex in lorObject.Children)
				{
					addLorObjectToElementList(m_parsedLorSequence.SequenceObjects[childIndex]);
				} // end process the children
			}
			// does this object exist in the Vixen Element list?
			else if (null == VixenSystem.Nodes.GetAllNodes().FirstOrDefault(x => x.Name == lorObject.Name))
			{
				CreateElementNodeAndParentTree(lorObject);
			}
		} // addLorObjectToElementList

		/// <summary>
		/// Create the parent tree for this node. Node MUST be a leaf node
		/// </summary>
		/// <param name="node"></param>
		/// <returns>The element node that was created</returns>
		private ElementNode CreateElementNodeAndParentTree(ILorObject lorObject)
		{
			// find the Vixen Element associated with this LOR Channel
			ElementNode response = VixenSystem.Nodes.GetAllNodes().FirstOrDefault(x => x.Name == lorObject.Name);

			// does this lor object already exist in the Vixen tree?
			if (null == response)
			{
				// no it does not exist. Create its parents and then create it
				foreach (UInt64 parentId in lorObject.Parents)
				{
					// get the parent object
					ILorObject parentObject = m_parsedLorSequence.SequenceObjects[parentId];

					// get the element for this parent
					ElementNode parentElement = CreateElementNodeAndParentTree(parentObject);

					// have we already created our element?
					if (null == response)
					{
						// create a new node and bind it to the parent
						response = ElementNodeService.Instance.CreateSingle(parentElement, lorObject.Name);
					}
					else
					{
						// bind the parent node to the existing child node
						VixenSystem.Nodes.AddChildToParent(response, parentElement);
					}
				} // end process parents

				// if there were no parents, then just make a top level node
				if (null == response)
				{
					// create a new node and bind it to the parent
					response = ElementNodeService.Instance.CreateSingle(null, lorObject.Name);
				} // end no parents
			} // end this Vixen node did not exist

			// return the node we created
			return response;
		} // CreateElementNodeParentTree

		/// <summary>
		/// Automatically map the selected LOR channels to the V3 Elements of the same name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonMapElements_Click(object sender, EventArgs e)
		{
			// reset the mapping count
			m_mappingCount = 0;

			// process the nodes that are currently selected
			foreach (TreeNode node in treeViewLorChannels.SelectedNodes)
			{
				addLorObjectToMap(m_parsedLorSequence.SequenceObjects[Convert.ToUInt64(node.Name)]);
			} // end process selected nodes

			MessageBox.Show("LOR Auto Map has updated " + m_mappingCount + " elements", "Map Vixen Elements");
		} // buttonMapElements_Click

		/// <summary>
		/// Map the leaf objects to Vixen elements of the same name
		/// </summary>
		/// <param name="lorObject"></param>
		private void addLorObjectToMap(ILorObject lorObject)
		{
			ElementNode element = null;

			// v3Destination
			if ("Mini Tree 1" == lorObject.Name)
			{
				Logging.Error("lorObject.Name: '" + lorObject.Name + "'");
			}

			// does this object have any children?
			if ((0 != lorObject.Children.Count) && (lorObject.GetType() != typeof(LorRgbChannel)))
			{
				// process any children the node may have
				foreach (UInt64 childIndex in lorObject.Children)
				{
					addLorObjectToMap(m_parsedLorSequence.SequenceObjects[childIndex]);
				} // end process the children
			}
			// does this object exist in the Vixen Element list?
			else if (null != (element = VixenSystem.Nodes.GetAllNodes().FirstOrDefault(x => x.Name == lorObject.Name)))
			{
				// v3Destination
				if ("Mini Tree 1" == element.ToString())
				{
					Logging.Error("element.Element: '" + element.Element + "'");
					Logging.Error("element.Element.Name: '" + element.Element.Name + "'");
				}

				if (null == element.Element)
				{
					Logging.Error("element.Element: '" + element.Element + "'");
				}
				// is this an RGB channel?
				if (lorObject.GetType() == typeof(LorRgbChannel))
				{
					// process the children
					foreach (UInt64 childIndex in lorObject.Children)
					{
						LorChannel rgbChild = m_parsedLorSequence.SequenceObjects[childIndex] as LorChannel;
						LorChannelMapping mapping = m_mappings.FirstOrDefault(x => x.ChannelName == rgbChild.Name);
						if (null != mapping)
						{
							m_mappingCount++;
							mapping.DestinationColor = rgbChild.Color;
							mapping.ColorMixing = true;
							mapping.ElementNodeId = element.Id;
						} // end map exists
					} // end RGB channels
				} // end RGB leaf
				else
				{
					// get the mapping entry for this element
					LorChannelMapping mapping = m_mappings.FirstOrDefault(x => x.ChannelName == lorObject.Name);
					if (null != mapping)
					{
						mapping.DestinationColor = (lorObject as LorChannel).Color;
						mapping.ColorMixing = false;
						mapping.ElementNodeId = element.Id;
					} // end map exists
				} // end there is an element for this lor channel
			} // end matching element exists
		} // addLorObjectToMap

		/// <summary>
		/// Done clean up and close
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonDone_Click(object sender, EventArgs e)
		{

		} // buttonDone_Click
	} // LightOramaAutoPopulateElementsForm
} // VixenModules.SequenceType.LightOrama
