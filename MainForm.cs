using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iTunesDuplicateFinder
{
    public partial class MainForm : Form
    {
        iTunesManager manager;
        public MainForm()
        {
            InitializeComponent();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            manager = new iTunesManager(albumCheckBox.Checked, deleteFilesCheckBox.Checked, caseSensitiveCheckBox.Checked, (double) lengthUpDown.Value, progressBar);
            manager.RemoveDuplicates();
        }
    }
}
