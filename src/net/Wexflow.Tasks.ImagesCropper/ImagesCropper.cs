using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesCropper
{
    public class ImagesCropper : Task
    {
        public int Width { get; }
        public int Height { get; }
        public int X { get; }
        public int Y { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public ImagesCropper(XElement xe, Workflow wf) : base(xe, wf)
        {
            Width = int.Parse(GetSetting("width"));
            Height = int.Parse(GetSetting("height"));
            X = int.Parse(GetSetting("x"));
            Y = int.Parse(GetSetting("y"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Cropping images...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CropImages(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = CropImages(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while cropping images.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool CropImages(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var images = SelectFiles();
                foreach (var image in images)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, image.FileName);
                    success &= Crop(image.Path, destPath);
                    if (!atLeastOneSuccess && success)
                    {
                        atLeastOneSuccess = true;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while cropping images: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool Crop(string srcPath, string destPath)
        {
            try
            {
                using (var src = Image.FromFile(srcPath))
                using (var dest = Crop(src, X, Y, Width, Height))
                {
                    dest.Save(destPath);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The image {0} was cropped -> {3}", srcPath, Width, Height, destPath);
                    return true;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while cropping the image {0}: {1}", srcPath, e.Message);
                return false;
            }
        }

        private Image Crop(Image src, int x, int y, int width, int height)
        {
            var cropRect = new Rectangle(x, y, width, height);
            var target = new Bitmap(cropRect.Width, cropRect.Height);

            using (var g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
    }
}
