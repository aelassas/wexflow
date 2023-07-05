using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesOverlay
{
    public class ImagesOverlay : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public ImagesOverlay(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Overlaying images...");
            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = Overlay();
                    }
                }
                else
                {
                    success = Overlay();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while copying files.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool Overlay()
        {
            var success = true;
            try
            {
                var imageFiles = SelectFiles();

                if (imageFiles.Length >= 2)
                {
                    var extension = Path.GetExtension(imageFiles[0].FileName);

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"ImagesOverlay_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}{extension}");

                    success = OverlayImages(imageFiles, destPath);
                }
                else if (imageFiles.Length == 1)
                {
                    Error("You must provide at least two images to overlay them.");
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while overlaying images: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool OverlayImages(FileInf[] imageFiles, string destPath)
        {
            try
            {
                var imageHeights = new List<int>();
                var imageWidths = new List<int>();

                foreach (var imageFile in imageFiles)
                {
                    using (var img = Image.FromFile(imageFile.Path))
                    {
                        imageHeights.Add(img.Height);
                        imageWidths.Add(img.Width);
                    }
                }
                imageHeights.Sort();
                imageWidths.Sort();

                var height = imageHeights[imageHeights.Count - 1];
                var width = imageWidths[imageWidths.Count - 1];

                using (var finalImage = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(finalImage))
                {
                    graphics.Clear(SystemColors.AppWorkspace);
                    foreach (var imageFile in imageFiles)
                    {
                        using (var img = Image.FromFile(imageFile.Path))
                        {
                            graphics.DrawImage(img, new Point(0, 0));
                        }
                    }

                    finalImage.Save(destPath);
                }

                InfoFormat("Image overlaying succeeded: {0}", destPath);
                Files.Add(new FileInf(destPath, Id));

                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while overlaying images: {0}", e.Message);
                return false;
            }
        }
    }
}
