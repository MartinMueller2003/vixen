using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VixenModules.SequenceType.LightOrama
{
	public partial class LightOramaAutoPopulateElementsForm : Form
	{
		/// <summary>
		/// Data used to access chanel information
		/// </summary>
		private LightOramaSequenceData m_parsedLorSequence = null;

		public LightOramaAutoPopulateElementsForm(LightOramaSequenceData parsedLorSequence)
		{
			InitializeComponent();
			m_parsedLorSequence = parsedLorSequence;
			PopulateLorChannelInformation();
		} // LightOramaAutoPopulateElementsForm

		/// <summary>
		///  Parse the LOR channel structure and create the channel lists. Ignore the channels that already have elements with matching names
		/// </summary>
		private void PopulateLorChannelInformation()
		{
			treeViewLorChannels.BeginUpdate();
			treeViewLorChannels.Nodes.Clear();

			TreeNode rootItem = null;
			List<TreeNode> children = new List<TreeNode>();

			// process all of the top level objects
			foreach (ILorObject lorObject in m_parsedLorSequence.SequenceObjects.Values.Where(x => x.HasParent == false))
			{
				children.Add(addLorChannelObject(lorObject));
			}

			// do we have children?
			if (0 != children.Count)
			{
				// this is a parent node
				rootItem = new TreeNode("Light-O-Rama Channels", children.ToArray());
			} // end parent node
			else
			{
				// this is a leaf node
				rootItem = new TreeNode("Light-O-Rama Channels");
			}

			treeViewLorChannels.Nodes.Add(rootItem);

			treeViewLorChannels.EndUpdate();
		} // PopulateLorChannelInformation

		/// <summary>
		/// Build the child node tree
		/// </summary>
		/// <param name="lorObject"></param>
		/// <returns></returns>
		private TreeNode addLorChannelObject(ILorObject parent)
		{
			TreeNode response = null;
			List<TreeNode> children = new List<TreeNode>();

			// process any children this object may have
			foreach (UInt64 childIndex in parent.Children)
			{
				// add the child to the list
				ILorObject child = m_parsedLorSequence.SequenceObjects[childIndex];
				children.Add(addLorChannelObject(child));
			} // end processing the children

			// do we have children?
			if( 0 != children.Count )
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

			return response;
		} // addLorChannelObject

		/// <summary>
		/// Create V3 Elements corresponding to the selected LOR channels
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonCreateElements_Click(object sender, EventArgs e)
		{

		} // buttonCreateElements_Click

		/// <summary>
		/// Automatically map the selected LOR channels to the V3 Elements of the same name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonMapElements_Click(object sender, EventArgs e)
		{

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
