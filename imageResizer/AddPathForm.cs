using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace imageResizer
{
    public partial class AddPathForm : Form
    {
        public AddPathForm()
        {
            InitializeComponent();
        }

        public string FolderPath { get; set; }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    this.txtDirectoryPath.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.FolderPath = this.txtDirectoryPath.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
