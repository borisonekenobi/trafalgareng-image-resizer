using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Octokit;

namespace imageResizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            getNewVersion();
            System.Windows.Forms.Application.Run(new Form1());
        }

        async static void getNewVersion()
        {
            var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            var releases = await client.Repository.Release.GetAll("Borisonekenobi", "imageResizer");
            var latest = releases[0];

            string[] versionNums = latest.TagName.Split('.');

            int major = int.Parse(versionNums[0].Split('v')[1]);
            int minor = int.Parse(versionNums[1]);
            int patch = int.Parse(versionNums[2]);
            Version latestVersion = new Version(major, minor, patch);
            Version version = new Version(System.Windows.Forms.Application.ProductVersion);

            if (latestVersion > version)
            {
                if (MessageBox.Show("A newer version of the software is available. Would you like to download it?", "New version available!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://github.com/borisonekenobi/imageResizer/releases/download/" + latest.TagName + "/imageResizer.exe");
                }
            }
        }
    }
}
