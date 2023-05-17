using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace imageResizer
{
    public partial class AboutBox : Form
    {
        internal Options Options {  get; set; }

        public AboutBox()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/borisonekenobi/");
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:borisonekenobi@gmail.com");
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            UpdateTheme();
            label5.Text = "Current: v" + Form1.CurrentVersion + "\nLatest: v" + Form1.LatestVersion;
        }

        private void UpdateTheme()
        {
            if (Options.Theme == 0)
            {
                SetLightTheme();
            }
            else if (Options.Theme == 1)
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

            buttonOK.BackColor = DefaultBackColor;
            buttonOK.FlatAppearance.BorderSize = 1;
        }

        private void SetDarkTheme()
        {
            UseImmersiveDarkMode(Handle, true);

            BackColor = backgroundColor;
            ForeColor = foregroundColor;

            buttonOK.BackColor = buttonColor;
            buttonOK.FlatAppearance.BorderSize = buttonBorderSize;
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
