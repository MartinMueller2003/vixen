namespace VixenModules.SequenceType.LightOrama
{
	partial class LightOramaSequenceImporterChannelMapper
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (treeview != null)
				{
					treeview.Dispose();
					treeview = null;
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("group1", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("group2", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("group3", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("qwerqewr");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "asdfasdf"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "sub1", System.Drawing.SystemColors.Info, System.Drawing.SystemColors.HotTrack, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)))),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "sub2")}, -1);
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("jjjjjjjjjjjjjj");
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("hhhhhhhhhhhhhh");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LightOramaSequenceImporterChannelMapper));
			this.listViewMapping = new System.Windows.Forms.ListView();
			this.lorChannel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lorChannelOutput = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lorChannelName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lorChannelColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.destinationElement = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.destinationColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RGBPixelColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonAutoPopulate = new System.Windows.Forms.Button();
			this.multiSelectTreeview1 = new Common.Controls.MultiSelectTreeview();
			this.destinationColorButton = new System.Windows.Forms.Button();
			this.labelMappingName = new System.Windows.Forms.Label();
			this.TextBoxMappingName = new System.Windows.Forms.TextBox();
			this.numericUpDownRepeatElements = new System.Windows.Forms.NumericUpDown();
			this.labelRepeatElements = new System.Windows.Forms.Label();
			this.checkBoxRGB = new System.Windows.Forms.CheckBox();
			this.comboBoxColorOrder = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeatElements)).BeginInit();
			this.SuspendLayout();
			// 
			// listViewMapping
			// 
			this.listViewMapping.AllowDrop = true;
			this.listViewMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.listViewMapping.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lorChannel,
            this.lorChannelOutput,
            this.lorChannelName,
            this.lorChannelColor,
            this.destinationElement,
            this.destinationColor,
            this.RGBPixelColumn});
			this.listViewMapping.FullRowSelect = true;
			this.listViewMapping.GridLines = true;
			listViewGroup1.Header = "group1";
			listViewGroup1.Name = "listViewGroup1";
			listViewGroup2.Header = "group2";
			listViewGroup2.Name = "listViewGroup2";
			listViewGroup3.Header = "group3";
			listViewGroup3.Name = "listViewGroup3";
			this.listViewMapping.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
			this.listViewMapping.HideSelection = false;
			this.listViewMapping.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4});
			this.listViewMapping.Location = new System.Drawing.Point(12, 12);
			this.listViewMapping.Name = "listViewMapping";
			this.listViewMapping.ShowGroups = false;
			this.listViewMapping.Size = new System.Drawing.Size(628, 488);
			this.listViewMapping.TabIndex = 0;
			this.listViewMapping.UseCompatibleStateImageBehavior = false;
			this.listViewMapping.View = System.Windows.Forms.View.Details;
			this.listViewMapping.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewMapping_DragDrop);
			this.listViewMapping.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewMapping_DragEnter);
			// 
			// LORChannel
			// 
			this.lorChannel.Text = "LOR Channel";
			this.lorChannel.Width = 68;
			// 
			// LORChannelOutput
			// 
			this.lorChannelOutput.Text = "LOR Output";
			this.lorChannelOutput.Width = 68;
			// 
			// LORChannelName
			// 
			this.lorChannelName.Text = "LOR Name";
			this.lorChannelName.Width = 130;
			// 
			// LORChannelColor
			// 
			this.lorChannelColor.Text = "LOR Color";
			this.lorChannelColor.Width = 70;
			// 
			// destinationElement
			// 
			this.destinationElement.Text = "Destination Element";
			this.destinationElement.Width = 150;
			// 
			// destinationColor
			// 
			this.destinationColor.Text = "Import Color";
			this.destinationColor.Width = 120;
			// 
			// RGBPixelColumn
			// 
			this.RGBPixelColumn.Text = "Color Mixing";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(759, 515);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(90, 25);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(855, 515);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(90, 25);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonAutoPopulate
			// 
			this.buttonAutoPopulate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAutoPopulate.Location = new System.Drawing.Point(650, 515);
			this.buttonAutoPopulate.Name = "buttonAutoPopulate";
			this.buttonAutoPopulate.Size = new System.Drawing.Size(90, 25);
			this.buttonAutoPopulate.TabIndex = 14;
			this.buttonAutoPopulate.Text = "Auto Populate";
			this.buttonAutoPopulate.UseVisualStyleBackColor = true;
			this.buttonAutoPopulate.Click += new System.EventHandler(this.buttonAutoPopulate_Click);
			// 
			// multiSelectTreeview1
			// 
			this.multiSelectTreeview1.AllowDrop = true;
			this.multiSelectTreeview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top |
																					   System.Windows.Forms.AnchorStyles.Bottom) |
																					   System.Windows.Forms.AnchorStyles.Left) |
																					   System.Windows.Forms.AnchorStyles.Right)));
			this.multiSelectTreeview1.CustomDragCursor = null;
			this.multiSelectTreeview1.DragDefaultMode = System.Windows.Forms.DragDropEffects.Move;
			this.multiSelectTreeview1.DragDestinationNodeBackColor = System.Drawing.SystemColors.Highlight;
			this.multiSelectTreeview1.DragDestinationNodeForeColor = System.Drawing.SystemColors.HighlightText;
			this.multiSelectTreeview1.DragSourceNodeBackColor = System.Drawing.SystemColors.ControlLight;
			this.multiSelectTreeview1.DragSourceNodeForeColor = System.Drawing.SystemColors.ControlText;
			this.multiSelectTreeview1.Location = new System.Drawing.Point(646, 12);
			this.multiSelectTreeview1.Name = "multiSelectTreeview1";
			this.multiSelectTreeview1.SelectedNodes = ((System.Collections.Generic.List<System.Windows.Forms.TreeNode>)(resources.GetObject("multiSelectTreeview1.SelectedNodes")));
			this.multiSelectTreeview1.Size = new System.Drawing.Size(299, 488);
			this.multiSelectTreeview1.TabIndex = 5;
			this.multiSelectTreeview1.UsingCustomDragCursor = false;
			this.multiSelectTreeview1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.multiSelectTreeview1_ItemDrag);
			this.multiSelectTreeview1.DragEnter += new System.Windows.Forms.DragEventHandler(this.multiSelectTreeview1_DragEnter);
			// 
			// destinationColorButton
			// 
			this.destinationColorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.destinationColorButton.Location = new System.Drawing.Point(245, 515);
			this.destinationColorButton.Name = "destinationColorButton";
			this.destinationColorButton.Size = new System.Drawing.Size(90, 25);
			this.destinationColorButton.TabIndex = 6;
			this.destinationColorButton.Text = "Set Import Color";
			this.destinationColorButton.UseVisualStyleBackColor = true;
			this.destinationColorButton.Click += new System.EventHandler(this.destinationColorButton_Click);
			// 
			// labelMappingName
			// 
			this.labelMappingName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelMappingName.AutoSize = true;
			this.labelMappingName.Location = new System.Drawing.Point(12, 521);
			this.labelMappingName.Name = "labelMappingName";
			this.labelMappingName.Size = new System.Drawing.Size(80, 13);
			this.labelMappingName.TabIndex = 7;
			this.labelMappingName.Text = "Mapping Name:";
			// 
			// TextBoxMappingName
			// 
			this.TextBoxMappingName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TextBoxMappingName.Location = new System.Drawing.Point(90, 515);
			this.TextBoxMappingName.Name = "TextBoxMappingName";
			this.TextBoxMappingName.Size = new System.Drawing.Size(150, 20);
			this.TextBoxMappingName.TabIndex = 8;
			// 
			// numericUpDownRepeatElements
			// 
			this.numericUpDownRepeatElements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.numericUpDownRepeatElements.Location = new System.Drawing.Point(450, 518);
			this.numericUpDownRepeatElements.Maximum = new decimal(new int[] 
			{
				20,
				0,
				0,
				0
			});
			this.numericUpDownRepeatElements.Minimum = new decimal(new int[] 
			{
				1,
				0,
				0,
				0
			});
			this.numericUpDownRepeatElements.Name = "numericUpDownRepeatElements";
			this.numericUpDownRepeatElements.Size = new System.Drawing.Size(30, 20);
			this.numericUpDownRepeatElements.TabIndex = 9;
			this.numericUpDownRepeatElements.Value = new decimal(new int[] 
			{
				1,
				0,
				0,
				0
			});
			// 
			// labelRepeatElements
			// 
			this.labelRepeatElements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelRepeatElements.AutoSize = true;
			this.labelRepeatElements.Location = new System.Drawing.Point(345, 522);
			this.labelRepeatElements.Name = "labelRepeatElements";
			this.labelRepeatElements.Size = new System.Drawing.Size(105, 13);
			this.labelRepeatElements.TabIndex = 10;
			this.labelRepeatElements.Text = "Repeat Elements:   x";
			// 
			// checkBoxRGB
			// 
			this.checkBoxRGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxRGB.AutoSize = true;
			this.checkBoxRGB.Location = new System.Drawing.Point(492, 515);
			this.checkBoxRGB.Name = "checkBoxRGB";
			this.checkBoxRGB.Size = new System.Drawing.Size(105, 21);
			this.checkBoxRGB.TabIndex = 11;
			this.checkBoxRGB.Text = "Color Mixing";
			this.checkBoxRGB.CheckedChanged += new System.EventHandler(this.checkBoxRGB_CheckedChanged);
			// 
			// comboBoxColorOrder
			// 
			this.comboBoxColorOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.comboBoxColorOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxColorOrder.FormattingEnabled = true;
			this.comboBoxColorOrder.Items.AddRange(new object[] 
			{
				"RGB",
				"RBG",
				"BRG",
				"BGR",
				"GRB",
				"GBR"
			});
			this.comboBoxColorOrder.Location = new System.Drawing.Point(575, 518);
			this.comboBoxColorOrder.Name = "comboBoxColorOrder";
			this.comboBoxColorOrder.Size = new System.Drawing.Size(65, 24);
			this.comboBoxColorOrder.TabIndex = 12;
			// 
			// LightOramaSequenceImporterChannelMapper
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(951, 552);
			this.Controls.Add(this.checkBoxRGB);
			this.Controls.Add(this.comboBoxColorOrder);
			this.Controls.Add(this.labelRepeatElements);
			this.Controls.Add(this.numericUpDownRepeatElements);
			this.Controls.Add(this.TextBoxMappingName);
			this.Controls.Add(this.labelMappingName);
			this.Controls.Add(this.destinationColorButton);
			this.Controls.Add(this.multiSelectTreeview1);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonAutoPopulate);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.listViewMapping);
			this.DoubleBuffered = true;
			this.Name = "LightOramaSequenceImporterChannelMapper";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Vixen 2.x Channel Mapping";
			this.Load += new System.EventHandler(this.LightOramaSequenceImporterChannelMapper_Load);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRepeatElements)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView listViewMapping;
		private System.Windows.Forms.ColumnHeader lorChannelName;
		private System.Windows.Forms.ColumnHeader lorChannelColor;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ColumnHeader lorChannel;
		private System.Windows.Forms.ColumnHeader lorChannelOutput;
		private System.Windows.Forms.ColumnHeader destinationElement;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonAutoPopulate;
		private Common.Controls.MultiSelectTreeview multiSelectTreeview1;
		private System.Windows.Forms.ColumnHeader destinationColor;
		private System.Windows.Forms.Button destinationColorButton;
		private System.Windows.Forms.Label labelMappingName;
		private System.Windows.Forms.TextBox TextBoxMappingName;
		private System.Windows.Forms.NumericUpDown numericUpDownRepeatElements;
		private System.Windows.Forms.Label labelRepeatElements;
		private System.Windows.Forms.ColumnHeader RGBPixelColumn;
		private System.Windows.Forms.CheckBox checkBoxRGB;
		private System.Windows.Forms.ComboBox comboBoxColorOrder;
	}
}