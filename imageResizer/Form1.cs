using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                tabControl1.SelectedIndex = 1;
                buttonBack.Enabled = true;
                buttonNext.Enabled = false;
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                tabControl1.SelectedIndex = 0;
                buttonBack.Enabled = false;
                buttonNext.Enabled = true;
            }
        }

        private Size calculateReducedSize(float horizontalRes, float verticalRes, int maxHorizontalRes, int maxVerticalRes)
        {
            float ratio = maxHorizontalRes / (float) horizontalRes;
            int calculatedVerticalRes = (int) (ratio * verticalRes);

            if (calculatedVerticalRes < maxVerticalRes)
            {
                return new Size(maxHorizontalRes, calculatedVerticalRes);
            }
            ratio = maxVerticalRes / (float) verticalRes;
            int calculatedHorizontalRes = (int) (ratio * horizontalRes);
            return new Size(calculatedHorizontalRes, maxVerticalRes);
        }

        private void ResizeImages(string sourcePath)
        {
            string[] files = Directory.GetFiles(sourcePath);
            string destinationPath = sourcePath + "\\reduced\\";

            System.IO.Directory.CreateDirectory(destinationPath);

            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);

                string sourceBitmapPath = sourcePath + "\\" + filename;
                string destinationBitmapPath = destinationPath + filename;

                try {
                    ResizeImage(sourceBitmapPath, destinationBitmapPath);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message + " - " + file);
                }
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            foreach (var path in this.foldersList.Items)
            {
                ResizeImages(path.ToString());
            }
            MessageBox.Show("Done!", "Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ResizeImage(string sourcePath, string destinationPath)
        {
            Image sourceBitmap = new Bitmap(sourcePath);

            var reducedSize = calculateReducedSize(sourceBitmap.Width, sourceBitmap.Height, 1280, 1024);

            var destRect = new Rectangle(0, 0, reducedSize.Width, reducedSize.Height);
            var destImage = new Bitmap(reducedSize.Width, reducedSize.Height);

            destImage.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(sourceBitmap, destRect, 0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            SaveJpg(destImage, destinationPath);
        }

        private void SaveJpg(Bitmap bitmap, string destinationPath)
        {
            // Get a bitmap.
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bitmap.Save(destinationPath, jgpEncoder, myEncoderParameters);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AddPathForm form = new AddPathForm();
            var result = form.ShowDialog();
            if (result == DialogResult.OK) this.foldersList.Items.Add(form.FolderPath);
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (this.foldersList.SelectedItems.Count == 0) return;
            this.foldersList.Items.Remove(this.foldersList.SelectedItem);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
