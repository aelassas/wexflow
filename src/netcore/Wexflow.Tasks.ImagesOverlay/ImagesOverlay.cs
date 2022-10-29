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
        public ImagesOverlay(XElement xe, Workflow wf)
          : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Overlaying images...");
            Status status = Status.Success;

            try
            {
                var imageFiles = SelectFiles();

                if (imageFiles.Length >= 2)
                {
                    var extension = Path.GetExtension(imageFiles[0].FileName);

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                            string.Format("ImagesOverlay_{0:yyyy-MM-dd-HH-mm-ss-fff}{1}", DateTime.Now, extension));

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
            catch (ThreadAbortException)
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
                List<int> imageHeights = new List<int>();
                List<int> imageWidths = new List<int>();
                
                foreach (FileInf imageFile in imageFiles)
                {
                    using (Image img = Image.FromFile(imageFile.Path))
                    {
                        imageHeights.Add(img.Height);
                        imageWidths.Add(img.Width);
                    }
                }
                imageHeights.Sort();
                imageWidths.Sort();

                int height = imageHeights[imageHeights.Count - 1];
                int width = imageWidths[imageWidths.Count - 1];

                using (Bitmap finalImage = new Bitmap(width, height))
                using (Graphics graphics = Graphics.FromImage(finalImage))
                {
                    graphics.Clear(SystemColors.AppWorkspace);
                    foreach (FileInf imageFile in imageFiles)
                    {
                        using (Image img = Image.FromFile(imageFile.Path))
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
