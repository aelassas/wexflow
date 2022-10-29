using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesResizer
{
    public class ImagesResizer : Task
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ImagesResizer(XElement xe, Workflow wf)
           : base(xe, wf)
        {
            Width = int.Parse(GetSetting("width"));
            Height = int.Parse(GetSetting("height"));
        }

        public override TaskStatus Run()
        {
            Info("Resizing images...");
            Status status = Status.Success;
            bool succeeded = true;
            bool atLeastOneSuccess = false;

            try
            {
                var images = SelectFiles();
                foreach (var image in images)
                {
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, image.FileName);
                    succeeded &= Resize(image.Path, destPath);
                    if (!atLeastOneSuccess && succeeded) atLeastOneSuccess = true;
                }

                if (!succeeded && atLeastOneSuccess)
                {
                    status = Status.Warning;
                }
                else if (!succeeded)
                {
                    status = Status.Error;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while resizing images: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool Resize(string srcPath, string destPath)
        {
            try
            {
                using (Image src = Image.FromFile(srcPath))
                using (Image dest = ResizeImage(src, Width, Height))
                {
                    dest.Save(destPath);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The image {0} was resized to {1}x{2} -> {3}", srcPath, Width, Height, destPath);
                    return true;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while resizing the image {0}: {1}", srcPath, e.Message);
                return false;
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

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
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

    }
}
