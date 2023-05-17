using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;
using Octokit;

namespace imageResizer
{
    public partial class Form1 : Form
    {
        public static Version CurrentVersion { get; set; }
        public static Version LatestVersion { get; set; }

        private bool checkedVersion = false;
        private Options options;
        private readonly string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageResizer");
        private readonly string fileFullPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageResizer"), "options.xml");
        
        public Form1()
        {
            InitializeComponent();
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(fileFullPath))
            {
                var serializer = new XmlSerializer(typeof(Options));
                using (var fs = new FileStream(fileFullPath, System.IO.FileMode.Open))
                {
                    options = (Options)serializer.Deserialize(fs);

                }

                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(options.NamingConvention) == -1 ? 0 : comboBox1.Items.IndexOf(options.NamingConvention);
                switch (options.MaxImageSize.Height)
                {
                    case 480: comboBox2.SelectedIndex = 0; break;
                    case 600: comboBox2.SelectedIndex = 1; break;
                    case 768: comboBox2.SelectedIndex = 2; break;
                    case 1024: comboBox2.SelectedIndex = 3; break;
                    default: comboBox2.SelectedIndex = 3; break;
                }
                comboBox3.SelectedIndex = comboBox3.Items.IndexOf(options.FolderName) == -1 ? 0 : comboBox3.Items.IndexOf(options.FolderName);

                switch (options.ReducedFolder)
                {
                    case 0: radioButton1.Checked = true; break;
                    case 1: radioButton2.Checked = true; break;
                    case 2: radioButton3.Checked = true; break;
                }

                numericUpDown1.Value = options.ImageQuality;

                switch (options.Theme)
                {
                    case 0: radioButton4.Checked = true; break;
                    case 1: radioButton5.Checked = true; break;
                    case 2: radioButton6.Checked = true; break;
                }
            }
            else
            {
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 3;
                comboBox3.SelectedIndex = 0;
            }

            UpdateTheme();
        }

        private void ButtonOptions_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                tabControl1.SelectedIndex = 1;
                buttonBack.Visible = true;
                buttonOptions.Visible = false;
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                tabControl1.SelectedIndex = 0;
                buttonBack.Visible = false;
                buttonOptions.Visible = true;
            }
            UpdateOptions();
        }

        private void ButtonRun_Click(object sender, EventArgs e)
        {
            if (foldersList.Items.Count == 0)
            {
                MessageBox.Show("Select at least 1 folder", "Run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdateOptions();

            int numImages = 0;
            foreach (var path in foldersList.Items)
            {
                DirectoryInfo dir = new DirectoryInfo(path.ToString());
                FileInfo[] imageFiles = dir.GetFiles("*.*");
                numImages += imageFiles.Length;
            }

            ProgressBars form = new ProgressBars
            {
                FolderList = foldersList.Items,
                NumFiles = numImages,
                Options = options
            };
            form.ShowDialog();
        }

        private void UpdateOptions()
        {
            options.NamingConvention = comboBox1.Text;
            switch (comboBox2.SelectedIndex)
            {
                case 0: options.MaxImageSize = new Size(640, 480); break;
                case 1: options.MaxImageSize = new Size(800, 600); break;
                case 2: options.MaxImageSize = new Size(1024, 768); break;
                case 3: options.MaxImageSize = new Size(1280, 1024); break;
                default: options.MaxImageSize = new Size(1280, 1024); break;
            }

            options.FolderName = comboBox3.Text;

            if (radioButton1.Checked) options.ReducedFolder = 0;
            else if (radioButton2.Checked) options.ReducedFolder = 1;
            else if (radioButton3.Checked) options.ReducedFolder = 2;

            options.ImageQuality = (long)numericUpDown1.Value;

            if (radioButton4.Checked) options.Theme = 0;
            else if (radioButton5.Checked) options.Theme = 1;
            else if (radioButton6.Checked) options.Theme = 2;

            if (!File.Exists(fileFullPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var doc = new XDocument();
            using (var writer = doc.CreateWriter())
            {
                var serializer = new XmlSerializer(typeof(Options));

                serializer.Serialize(writer, options);
            }

            doc.Save(fileFullPath);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
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

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FoldersList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory) foldersList.Items.Add(filePath);
                }
            }
        }

        private void FoldersList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ButtonAbout_Click(object sender, EventArgs e)
        {
            new AboutBox
            {
                Options = options
            }.ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            for (int i = foldersList.Items.Count - 1; i >= 0; i--)
            {
                foldersList.Items.RemoveAt(i);
            }
        }

        private void FoldersList_KeyDown(object sender, KeyEventArgs e)
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
            for (int i = foldersList.SelectedIndices.Count - 1; i >= 0; i--)
            {
                foldersList.Items.RemoveAt(foldersList.SelectedIndices[i]);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!checkedVersion)
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
                LatestVersion = new Version(major, minor, patch);
                CurrentVersion = new Version(System.Windows.Forms.Application.ProductVersion);

                if (LatestVersion > CurrentVersion)
                {
                    if (MessageBox.Show(this, "A newer version of the software is available. Would you like to download it?", "New version available!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://github.com/borisonekenobi/imageResizer/releases/download/" + latest.TagName + "/imageResizer.msi");
                    }
                }
                checkedVersion = true;
            }
        }

        private void Theme_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) options.Theme = 0;
            else if (radioButton5.Checked) options.Theme = 1;
            else if (radioButton6.Checked) options.Theme = 2;
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            if (options.Theme == 0)
            {
                SetLightTheme();
            }
            else if (options.Theme == 1)
            {
                SetDarkTheme();
            }
            else
            {
                if (UsingLightTheme())
                {
                    SetLightTheme();
                }
                else
                {
                    SetDarkTheme();
                }
            }
        }

        private void SetLightTheme()
        {
            UseImmersiveDarkMode(Handle, false);

            BackColor = DefaultBackColor;
            ForeColor = DefaultForeColor;

            tabPage1.BackColor = DefaultBackColor;
            tabPage1.ForeColor = DefaultForeColor;
            tabPage2.BackColor = DefaultBackColor;
            tabPage2.ForeColor = DefaultForeColor;

            foldersList.BackColor = DefaultBackColor;
            foldersList.ForeColor = DefaultForeColor;

            button1.BackColor = DefaultBackColor;
            button1.FlatAppearance.BorderSize = 1;

            buttonDelete.BackColor = DefaultBackColor;
            buttonDelete.FlatAppearance.BorderSize = 1;

            buttonAdd.BackColor = DefaultBackColor;
            buttonAdd.FlatAppearance.BorderSize = 1;

            buttonAbout.BackColor = DefaultBackColor;
            buttonAbout.FlatAppearance.BorderSize = 1;

            buttonOptions.BackColor = DefaultBackColor;
            buttonOptions.FlatAppearance.BorderSize = 1;

            buttonBack.BackColor = DefaultBackColor;
            buttonBack.FlatAppearance.BorderSize = 1;

            buttonCancel.BackColor = DefaultBackColor;
            buttonCancel.FlatAppearance.BorderSize = 1;

            buttonRun.BackColor = DefaultBackColor;
            buttonRun.FlatAppearance.BorderSize = 1;

            comboBox1.BackColor = SystemColors.Window;
            comboBox1.ForeColor = SystemColors.WindowText;

            comboBox2.BackColor = SystemColors.Window;
            comboBox2.ForeColor = SystemColors.WindowText;

            comboBox3.BackColor = SystemColors.Window;
            comboBox3.ForeColor = SystemColors.WindowText;

            numericUpDown1.BackColor = SystemColors.Window;
            numericUpDown1.ForeColor = SystemColors.WindowText;
        }

        private void SetDarkTheme()
        {
            UseImmersiveDarkMode(Handle, true);

            BackColor = backgroundColor;
            ForeColor = foregroundColor;

            tabPage1.BackColor = backgroundColor;
            tabPage1.ForeColor = foregroundColor;
            tabPage2.BackColor = backgroundColor;
            tabPage2.ForeColor = foregroundColor;

            foldersList.BackColor = backgroundColor;
            foldersList.ForeColor = foregroundColor;

            button1.BackColor = buttonColor;
            button1.FlatAppearance.BorderSize = buttonBorderSize;

            buttonDelete.BackColor = buttonColor;
            buttonDelete.FlatAppearance.BorderSize = buttonBorderSize;

            buttonAdd.BackColor = buttonColor;
            buttonAdd.FlatAppearance.BorderSize = buttonBorderSize;

            buttonAbout.BackColor = buttonColor;
            buttonAbout.FlatAppearance.BorderSize = buttonBorderSize;

            buttonOptions.BackColor = buttonColor;
            buttonOptions.FlatAppearance.BorderSize = buttonBorderSize;

            buttonBack.BackColor = buttonColor;
            buttonBack.FlatAppearance.BorderSize = buttonBorderSize;

            buttonCancel.BackColor = buttonColor;
            buttonCancel.FlatAppearance.BorderSize = buttonBorderSize;

            buttonRun.BackColor = buttonColor;
            buttonRun.FlatAppearance.BorderSize = buttonBorderSize;

            comboBox1.BackColor = buttonColor;
            comboBox1.ForeColor = foregroundColor;

            comboBox2.BackColor = buttonColor;
            comboBox2.ForeColor = foregroundColor;

            comboBox3.BackColor = buttonColor;
            comboBox3.ForeColor = foregroundColor;

            numericUpDown1.BackColor = buttonColor;
            numericUpDown1.ForeColor = foregroundColor;
        }

        private static bool UsingLightTheme()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var appsUseLightTheme = registryKey?.GetValue("AppsUseLightTheme");

            if (appsUseLightTheme is null)
            {
                return true;
            }
            else
            {
                return Convert.ToBoolean(appsUseLightTheme, CultureInfo.InvariantCulture);
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        internal static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        // styles for dark mode
        Color backgroundColor = Color.FromArgb(12, 12, 12);
        Color foregroundColor = Color.White;
        Color buttonColor = Color.FromArgb(44, 44, 44);
        int buttonBorderSize = 0;
    }
}
