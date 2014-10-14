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
	public partial class CoversionProgressForm : Form
	{
		public string StatusLineLabel
		{
			set { lblStatusLine.Text = value; }
		}


		public CoversionProgressForm()
		{
			InitializeComponent();
		}

		public void UpdateProgressBar(int value)
		{
			pbImport.Value = value;
		}

		/// <summary>
		/// Increment the progress bar if it is not at 100%
		/// </summary>
		public void IncrementProgressBar()
		{
			// can the value be incremented?
			if (pbImport.Maximum > pbImport.Value)
			{
				pbImport.Value++;
			}
		} // IncrementProgressBar

		public void SetupProgressBar(int min, int max)
		{
			pbImport.Minimum = min;
			pbImport.Maximum = max;
			pbImport.Value = 0;
		}
	}
}