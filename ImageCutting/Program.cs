using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ImageCutting
{
    public enum Mode
    {
        Cut,
        Margin,
    }

    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Mode mode = Mode.Cut;

            //Select image
            string imagePath = ShowFileDialog();

            Console.WriteLine("Select image to edit");
            if (string.IsNullOrEmpty(imagePath))
                throw new Exception("No valid image selected");

            //Select save path
            Console.WriteLine("Select save folder location");
            string savePath = ShowFolderDialog();

            if (string.IsNullOrEmpty(savePath))
                throw new Exception("No valid safe path selected");

            Image originalImage = Image.FromFile(imagePath);

            //Image size to cut out
            var cropWidth = 8;
            var cropHeight = 8;

            var x = originalImage.Width / cropWidth;
            var y = originalImage.Height / cropHeight;

            Bitmap largerImage = new Bitmap(originalImage.Width * 2, originalImage.Height * 2);
            using (Graphics gfx = Graphics.FromImage(largerImage))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0)))
            {
                gfx.FillRectangle(brush, 0, 0, largerImage.Width, largerImage.Height);
            }

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    //Area of each frame
                    Rectangle cropArea = new Rectangle(cropWidth * i, cropHeight * j, cropWidth, cropHeight);

                    //Cut out the frame
                    Bitmap croppedImage = new Bitmap(cropArea.Width, cropArea.Height);
                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        g.DrawImage(originalImage, new Rectangle(0, 0, cropArea.Width, cropArea.Height),
                                    cropArea,
                                    GraphicsUnit.Pixel);
                    }

                    CopyRegionIntoImage(croppedImage, new Rectangle(0, 0, cropArea.Width, cropArea.Height), ref largerImage, new Rectangle(cropWidth * 2 * i, cropHeight * 2 * j, cropWidth, cropHeight));

                    //Save the cropped frame
                    if (mode == Mode.Cut)
                    {
                        string croppedImagePath = String.Format(@"{0}\Image{1}x{2}_{3}.png", savePath, cropWidth, cropHeight, i + (j * y));
                        croppedImage.Save(croppedImagePath);
                    }
                }
            }

            if (mode == Mode.Margin)
            {
                string largeImagePath = string.Format(@"{0}\LargeImage.png", savePath);
                largerImage.Save(largeImagePath);
            }

        }

        /// <summary>
        /// Shows OpenFileDialog and returns selected file path
        /// </summary>
        /// <returns>The selected Files Path</returns>
        static string ShowFileDialog()
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            return filePath;
        }

        /// <summary>
        /// Shows windows folder select dialog and returns selected folder
        /// </summary>
        /// <returns>The selected Folder</returns>
        static string ShowFolderDialog()
        {
            var folder = string.Empty;

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    folder =  fbd.SelectedPath;
                }
            }

            return folder;
        }

        /// <summary>
        /// Pastes a region from one image to another
        /// </summary>
        /// <param name="srcBitmap"></param>
        /// <param name="srcRegion"></param>
        /// <param name="destBitmap"></param>
        /// <param name="destRegion"></param>
        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
    }

}

