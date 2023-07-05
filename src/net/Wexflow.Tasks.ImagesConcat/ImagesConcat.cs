using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesConcat
{
    public class ImagesConcat : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public ImagesConcat(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Concatenating images...");

            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = Concat();
                    }
                }
                else
                {
                    success = Concat();
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

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool Concat()
        {
            var success = true;
            try
            {
                var imageFiles = SelectFiles();

                if (imageFiles.Length >= 2)
                {
                    var extension = Path.GetExtension(imageFiles[0].FileName);

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"ImagesConcat_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}{extension}");

                    success = ConcatImages(imageFiles, destPath);
                }
                else if (imageFiles.Length == 1)
                {
                    Error("You must provide at least two images to concatenate them.");
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while concatenating images: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool ConcatImages(FileInf[] imageFiles, string destPath)
        {
            try
            {
                var imageHeights = new List<int>();
                var nIndex = 0;
                var width = 0;
                foreach (var imageFile in imageFiles)
                {
                    using (var img = Image.FromFile(imageFile.Path))
                    {
                        imageHeights.Add(img.Height);
                        width += img.Width;
                    }
                }
                imageHeights.Sort();

                var height = imageHeights[imageHeights.Count - 1];

                using (var finalImage = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(finalImage))
                {
                    graphics.Clear(SystemColors.AppWorkspace);
                    foreach (var imageFile in imageFiles)
                    {
                        using (var img = Image.FromFile(imageFile.Path))
                        {
                            if (nIndex == 0)
                            {
                                graphics.DrawImage(img, new Point(0, 0));
                                nIndex++;
                                width = img.Width;
                            }
                            else
                            {
                                graphics.DrawImage(img, new Point(width, 0));
                                width += img.Width;
                            }
                        }
                    }

                    finalImage.Save(destPath);
                }

                InfoFormat("Image concatenation succeeded: {0}", destPath);
                Files.Add(new FileInf(destPath, Id));

                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while concatenating images: {0}", e.Message);
                return false;
            }
        }
    }
}
