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

		private int m_mappingCount = 0;

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
				// ask if we should create elements
				if (false == m_parsedLorSequence.CleanUpDuplicates())
				{
					// cannot continue. Move to manual processing mode
					break;
				} // end failed to resolve the existance of duplicates in the element names

				int startingElementCount = VixenSystem.Nodes.GetAllNodes().Count();

				// process the nodes that are currently selected
				foreach (TreeNode node in treeViewLorChannels.SelectedNodes)
				{
					m_parsedLorSequence.addLorObjectToElementList(m_parsedLorSequence.SequenceObjects[Convert.ToUInt64(node.Name)]);
				} // end process selected nodes

				MessageBox.Show("LOR Auto Populate has created " + (VixenSystem.Nodes.GetAllNodes().Count() - startingElementCount) + " elements", "Create Vixen Elements");
				Logging.Info("LOR Auto Populate has created " + (VixenSystem.Nodes.GetAllNodes().Count() - startingElementCount) + " elements");
			} while (false);
		} // buttonCreateElements_Click

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
				m_mappingCount += m_parsedLorSequence.addLorObjectToMap(m_parsedLorSequence.SequenceObjects[Convert.ToUInt64(node.Name)]);
			} // end process selected nodes

			MessageBox.Show("LOR Auto Map has updated " + m_mappingCount + " elements", "Map Vixen Elements");
		} // buttonMapElements_Click

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
