namespace VixenModules.SequenceType.LightOrama
{
	partial class LightOramaAutoPopulateElementsForm
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
			this.components = new System.ComponentModel.Container();
			this.buttonDone = new System.Windows.Forms.Button();
			this.buttonCreateElements = new System.Windows.Forms.Button();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.buttonMapElements = new System.Windows.Forms.Button();
			this.treeViewLorChannels = new Common.Controls.MultiSelectTreeview();
			this.SuspendLayout();
			// 
			// treeViewLorChannels
			// 
			this.treeViewLorChannels.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewLorChannels.Location = new System.Drawing.Point(26, 23);
			this.treeViewLorChannels.Name = "treeViewLorChannels";
			this.treeViewLorChannels.Size = new System.Drawing.Size(455, 501);
			this.treeViewLorChannels.TabIndex = 0;
			this.toolTip.SetToolTip(treeViewLorChannels, "Light-O-Rama Channels");
			// 
			// buttonDone
			// 
			this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonDone.Location = new System.Drawing.Point(406, 546);
			this.buttonDone.Name = "buttonDone";
			this.buttonDone.Size = new System.Drawing.Size(75, 23);
			this.buttonDone.TabIndex = 3;
			this.buttonDone.Text = "Done";
			this.toolTip.SetToolTip(this.buttonDone, "Close Window. Return to Mapping screen");
			this.buttonDone.UseVisualStyleBackColor = true;
			this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
			// 
			// buttonCreateElements
			// 
			this.buttonCreateElements.Location = new System.Drawing.Point(26, 546);
			this.buttonCreateElements.Name = "buttonCreateElements";
			this.buttonCreateElements.Size = new System.Drawing.Size(138, 23);
			this.buttonCreateElements.TabIndex = 1;
			this.buttonCreateElements.Text = "Create Element(s)";
			this.toolTip.SetToolTip(this.buttonCreateElements, "Create Selected Elements\r\n");
			this.buttonCreateElements.UseVisualStyleBackColor = true;
			this.buttonCreateElements.Click += new System.EventHandler(this.buttonCreateElements_Click);
			// 
			// buttonMapElements
			// 
			this.buttonMapElements.Location = new System.Drawing.Point(221, 546);
			this.buttonMapElements.Name = "buttonMapElements";
			this.buttonMapElements.Size = new System.Drawing.Size(116, 23);
			this.buttonMapElements.TabIndex = 2;
			this.buttonMapElements.Text = "Map Element(s)";
			this.toolTip.SetToolTip(this.buttonMapElements, "Map selected channels to Vixen 3 Elements of the same name");
			this.buttonMapElements.UseVisualStyleBackColor = true;
			this.buttonMapElements.Click += new System.EventHandler(this.buttonMapElements_Click);
			// 
			// LightOramaAutoPopulateElementsForm
			// 
			this.AcceptButton = this.buttonDone;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(578, 583);
			this.Controls.Add(treeViewLorChannels);
			this.Controls.Add(this.buttonMapElements);
			this.Controls.Add(this.buttonCreateElements);
			this.Controls.Add(this.buttonDone);
			this.Name = "LightOramaAutoPopulateElementsForm";
			this.Text = "Light-O-Rama Auto Populate Elements";
			this.toolTip.SetToolTip(this, "Add / Map LOR Channels to Vixen 3 Elements");
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonDone;
		private System.Windows.Forms.Button buttonCreateElements;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Button buttonMapElements;
		private Common.Controls.MultiSelectTreeview treeViewLorChannels;


	}
}