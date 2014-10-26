namespace VixenModules.SequenceType.LightOrama
{
	partial class LightOramaSequenceImporterForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.sequenceToConvertLabel = new System.Windows.Forms.Label();
			this.lightOramaSequenceTextBox = new System.Windows.Forms.TextBox();
			this.lightOramaProfileLabel = new System.Windows.Forms.Label();
			this.createMapButton = new System.Windows.Forms.Button();
			this.convertButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.labelSelectedMap = new System.Windows.Forms.Label();
			this.lightOramaToVixen3MappingListBox = new System.Windows.Forms.ListBox();
			this.lightOramaProfileTextBox = new System.Windows.Forms.TextBox();
			this.lightOramaToVixen3MappingTextBox = new System.Windows.Forms.TextBox();
			this.labelChannelMaps = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// sequenceToConvertLabel
			// 
			this.sequenceToConvertLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sequenceToConvertLabel.AutoSize = true;
			this.sequenceToConvertLabel.Location = new System.Drawing.Point(152, 11);
			this.sequenceToConvertLabel.Name = "sequenceToConvertLabel";
			this.sequenceToConvertLabel.Size = new System.Drawing.Size(97, 13);
			this.sequenceToConvertLabel.TabIndex = 4;
			this.sequenceToConvertLabel.Text = "Light-O-Rama Sequence:";
			// 
			// LightOramaSequenceTextBox
			// 
			this.lightOramaSequenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lightOramaSequenceTextBox.Location = new System.Drawing.Point(155, 27);
			this.lightOramaSequenceTextBox.Name = "LightOramaSequenceTextBox";
			this.lightOramaSequenceTextBox.ReadOnly = true;
			this.lightOramaSequenceTextBox.Size = new System.Drawing.Size(454, 20);
			this.lightOramaSequenceTextBox.TabIndex = 5;
			// 
			// LightOramaProfileLabel
			// 
			this.lightOramaProfileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lightOramaProfileLabel.AutoSize = true;
			this.lightOramaProfileLabel.Location = new System.Drawing.Point(152, 54);
			this.lightOramaProfileLabel.Name = "LightOramaProfileLabel";
			this.lightOramaProfileLabel.Size = new System.Drawing.Size(77, 13);
			this.lightOramaProfileLabel.TabIndex = 7;
			this.lightOramaProfileLabel.Text = "Light-O-Rama Profile:";
			// 
			// createMapButton
			// 
			this.createMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.createMapButton.Location = new System.Drawing.Point(12, 149);
			this.createMapButton.Name = "createMapButton";
			this.createMapButton.Size = new System.Drawing.Size(127, 25);
			this.createMapButton.TabIndex = 13;
			this.createMapButton.Text = "Create New Map";
			this.createMapButton.UseVisualStyleBackColor = true;
			this.createMapButton.Click += new System.EventHandler(this.createMapButton_Click);
			// 
			// convertButton
			// 
			this.convertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.convertButton.Enabled = false;
			this.convertButton.Location = new System.Drawing.Point(155, 149);
			this.convertButton.Name = "convertButton";
			this.convertButton.Size = new System.Drawing.Size(110, 25);
			this.convertButton.TabIndex = 14;
			this.convertButton.Text = "Convert Sequence";
			this.convertButton.UseVisualStyleBackColor = true;
			this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteButton.Enabled = false;
			this.deleteButton.Location = new System.Drawing.Point(275, 149);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(110, 25);
			this.deleteButton.TabIndex = 15;
			this.deleteButton.Text = "Delete Mapping";
			this.deleteButton.UseVisualStyleBackColor = true;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(519, 149);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(90, 25);
			this.cancelButton.TabIndex = 16;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// labelSelectedMap
			// 
			this.labelSelectedMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSelectedMap.AutoSize = true;
			this.labelSelectedMap.Location = new System.Drawing.Point(152, 97);
			this.labelSelectedMap.Name = "labelSelectedMap";
			this.labelSelectedMap.Size = new System.Drawing.Size(76, 13);
			this.labelSelectedMap.TabIndex = 17;
			this.labelSelectedMap.Text = "Selected Map:";
			// 
			// LightOramaToVixen3MappingListBox
			// 
			this.lightOramaToVixen3MappingListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lightOramaToVixen3MappingListBox.Location = new System.Drawing.Point(12, 27);
			this.lightOramaToVixen3MappingListBox.Name = "LightOramaToVixen3MappingListBox";
			this.lightOramaToVixen3MappingListBox.Size = new System.Drawing.Size(127, 108);
			this.lightOramaToVixen3MappingListBox.TabIndex = 18;
			this.lightOramaToVixen3MappingListBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lightOramaToVixen3MappingListBox_MouseClick);
			// 
			// LightOramaProfileTextBox
			// 
			this.lightOramaProfileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lightOramaProfileTextBox.Location = new System.Drawing.Point(155, 70);
			this.lightOramaProfileTextBox.Name = "LightOramaProfileTextBox";
			this.lightOramaProfileTextBox.ReadOnly = true;
			this.lightOramaProfileTextBox.Size = new System.Drawing.Size(454, 20);
			this.lightOramaProfileTextBox.TabIndex = 8;
			// 
			// LightOramaToVixen3MappingTextBox
			// 
			this.lightOramaToVixen3MappingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lightOramaToVixen3MappingTextBox.Location = new System.Drawing.Point(155, 113);
			this.lightOramaToVixen3MappingTextBox.Name = "LightOramaToVixen3MappingTextBox";
			this.lightOramaToVixen3MappingTextBox.ReadOnly = true;
			this.lightOramaToVixen3MappingTextBox.Size = new System.Drawing.Size(454, 20);
			this.lightOramaToVixen3MappingTextBox.TabIndex = 18;
			// 
			// labelChannelMaps
			// 
			this.labelChannelMaps.AutoSize = true;
			this.labelChannelMaps.Location = new System.Drawing.Point(12, 12);
			this.labelChannelMaps.Name = "labelChannelMaps";
			this.labelChannelMaps.Size = new System.Drawing.Size(78, 13);
			this.labelChannelMaps.TabIndex = 19;
			this.labelChannelMaps.Text = "Channel Maps:";
			// 
			// LightOramaSequenceImporterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(621, 186);
			this.Controls.Add(this.labelChannelMaps);
			this.Controls.Add(this.lightOramaToVixen3MappingTextBox);
			this.Controls.Add(this.lightOramaToVixen3MappingListBox);
			this.Controls.Add(this.labelSelectedMap);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.convertButton);
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.createMapButton);
			this.Controls.Add(this.lightOramaProfileTextBox);
			this.Controls.Add(this.lightOramaProfileLabel);
			this.Controls.Add(this.lightOramaSequenceTextBox);
			this.Controls.Add(this.sequenceToConvertLabel);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "LightOramaSequenceImporterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Light-O-Rama to Vixen 3 Converter";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label sequenceToConvertLabel;
		private System.Windows.Forms.TextBox lightOramaSequenceTextBox;
        private System.Windows.Forms.Label lightOramaProfileLabel;
        private System.Windows.Forms.Button createMapButton;
		private System.Windows.Forms.Button convertButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label labelSelectedMap;
		private System.Windows.Forms.ListBox lightOramaToVixen3MappingListBox;
		private System.Windows.Forms.TextBox lightOramaProfileTextBox;
		private System.Windows.Forms.TextBox lightOramaToVixen3MappingTextBox;
		private System.Windows.Forms.Label labelChannelMaps;
	}
}