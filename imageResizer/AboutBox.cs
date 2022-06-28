using System;
using System.Windows.Forms;

namespace imageResizer
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/borisonekenobi/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:borisonekenobi@gmail.com");
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            label5.Text = "Current: v" + Form1.CurrentVersion + "\nLatest: v" + Form1.LatestVersion;
        }
    }
}
