using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesCropper
{
    [SupportedOSPlatform("windows")]
    public class ImagesCropper : Task
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public ImagesCropper(XElement xe, Workflow wf)
           : base(xe, wf)
        {
            Width = int.Parse(GetSetting("width"));
            Height = int.Parse(GetSetting("height"));
            X = int.Parse(GetSetting("x"));
            Y = int.Parse(GetSetting("y"));
        }

        public override TaskStatus Run()
        {
            Info("Cropping images...");
            var status = Status.Success;
            var succeeded = true;
            var atLeastOneSuccess = false;

            try
            {
                var images = SelectFiles();
                foreach (var image in images)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, image.FileName);
                    succeeded &= Crop(image.Path, destPath);
                    if (!atLeastOneSuccess && succeeded)
                    {
                        atLeastOneSuccess = true;
                    }
                    WaitOne();
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
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while cropping images: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool Crop(string srcPath, string destPath)
        {
            try
            {
                using var src = Image.FromFile(srcPath);
                using var dest = Crop(src, X, Y, Width, Height);
                dest.Save(destPath);
                Files.Add(new FileInf(destPath, Id));
                InfoFormat("The image {0} was cropped -> {3}", srcPath, Width, Height, destPath);
                return true;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while cropping the image {0}: {1}", srcPath, e.Message);
                return false;
            }
        }

        private static Image Crop(Image src, int x, int y, int width, int height)
        {
            Rectangle cropRect = new(x, y, width, height);
            Bitmap target = new(cropRect.Width, cropRect.Height);

            using var g = Graphics.FromImage(target);
            g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                cropRect,
                GraphicsUnit.Pixel);
            return target;
        }
    }
}
