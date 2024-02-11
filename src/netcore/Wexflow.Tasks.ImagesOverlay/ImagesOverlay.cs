using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesOverlay
{
    [SupportedOSPlatform("windows")]
    public class ImagesOverlay : Task
    {
        public ImagesOverlay(XElement xe, Workflow wf)
          : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Overlaying images...");
            var status = Status.Success;

            try
            {
                var imageFiles = SelectFiles();

                if (imageFiles.Length >= 2)
                {
                    var extension = Path.GetExtension(imageFiles[0].FileName);

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"ImagesOverlay_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}{extension}");

                    var res = OverlayImages(imageFiles, destPath);
                    if (!res)
                    {
                        status = Status.Error;
                    }
                }
                else if (imageFiles.Length == 1)
                {
                    Error("You must provide at least two images to overlay them.");
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while overlaying images: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool OverlayImages(FileInf[] imageFiles, string destPath)
        {
            try
            {
                List<int> imageHeights = new();
                List<int> imageWidths = new();

                foreach (var imageFile in imageFiles)
                {
                    using var img = Image.FromFile(imageFile.Path);
                    imageHeights.Add(img.Height);
                    imageWidths.Add(img.Width);
                }
                imageHeights.Sort();
                imageWidths.Sort();

                var height = imageHeights[^1];
                var width = imageWidths[^1];

                using (Bitmap finalImage = new(width, height))
                using (var graphics = Graphics.FromImage(finalImage))
                {
                    graphics.Clear(SystemColors.AppWorkspace);
                    foreach (var imageFile in imageFiles)
                    {
                        using var img = Image.FromFile(imageFile.Path);
                        graphics.DrawImage(img, new Point(0, 0));
                        WaitOne();
                    }

                    finalImage.Save(destPath);
                }

                InfoFormat("Image overlaying succeeded: {0}", destPath);
                Files.Add(new FileInf(destPath, Id));

                return true;
            }
            catch (ThreadInterruptedException)
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
