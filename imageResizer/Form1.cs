using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Octokit;

namespace imageResizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                tabControl1.SelectedIndex = 1;
                buttonBack.Visible = true;
                buttonOptions.Visible = false;
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                tabControl1.SelectedIndex = 0;
                buttonBack.Visible = false;
                buttonOptions.Visible = true;
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (this.foldersList.Items.Count == 0)
            {
                MessageBox.Show("Select at least 1 folder", "Run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var options = new Options();
            options.NamingConvention = this.comboBox1.Text;
            switch (this.comboBox2.SelectedIndex)
            {
                case 0: options.MaxImageSize = new Size(640, 480); break;
                case 1: options.MaxImageSize = new Size(800, 600); break;
                case 2: options.MaxImageSize = new Size(1024, 768); break;
                case 3: options.MaxImageSize = new Size(1280, 1024); break;
                default: options.MaxImageSize = new Size(1280, 1024); break;
            }
            options.FolderName = this.comboBox3.Text;
            if (this.radioButton1.Checked) options.ReducedFolder = 0;
            else if (this.radioButton2.Checked) options.ReducedFolder = 1;
            else if (this.radioButton3.Checked) options.ReducedFolder = 2;
            options.ImageQuality = (long) this.numericUpDown1.Value;

            int numImages = 0;
            foreach (var path in this.foldersList.Items)
            {
                DirectoryInfo dir = new DirectoryInfo(path.ToString());
                FileInfo[] imageFiles = dir.GetFiles("*.*");
                numImages += imageFiles.Length;
            }

            ProgressBars form = new ProgressBars
            {
                FolderList = this.foldersList.Items,
                NumFiles = numImages,
                Options = options
            };
            form.ShowDialog();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    foldersList.Items.Add(folderBrowserDialog.SelectedPath);
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void foldersList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory) this.foldersList.Items.Add(filePath);
                }
            }
        }

        private void foldersList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 3;
            comboBox3.SelectedIndex = 0;
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = this.foldersList.Items.Count - 1; i >= 0; i--)
            {
                this.foldersList.Items.RemoveAt(i);
            }
        }

        private void foldersList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                for (int i = 0; i < foldersList.Items.Count; i++)
                {
                    foldersList.SetSelected(i, true);
                }
            }

            if (e.KeyCode == Keys.Delete)
            {
                Delete();
            }
        }

        private void Delete()
        {
            for (int i = this.foldersList.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.foldersList.Items.RemoveAt(this.foldersList.SelectedIndices[i]);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            var task = client.Repository.Release.GetAll("Borisonekenobi", "imageResizer");
            task.Wait();
            var releases = task.Result;
            var latest = releases[0];

            string[] versionNums = latest.TagName.Split('.');

            int major = int.Parse(versionNums[0].Split('v')[1]);
            int minor = int.Parse(versionNums[1]);
            int patch = int.Parse(versionNums[2]);
            Version latestVersion = new Version(major, minor, patch);
            Version version = new Version(System.Windows.Forms.Application.ProductVersion);

            if (latestVersion > version)
            {
                if (MessageBox.Show(this, "A newer version of the software is available. Would you like to download it?", "New version available!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://github.com/borisonekenobi/imageResizer/releases/download/" + latest.TagName + "/imageResizer.exe");
                }
            }
        }
    }
}
