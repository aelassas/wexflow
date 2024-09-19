using SkiaSharp;
using System;
using System.Collections.Generic;
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
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"ImagesOverlay_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.png");

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
                List<int> imageHeights = [];
                List<int> imageWidths = [];

                foreach (var imageFile in imageFiles)
                {
                    using var img = SKImage.FromEncodedData(imageFile.Path);
                    imageHeights.Add(img.Height);
                    imageWidths.Add(img.Width);
                }
                imageHeights.Sort();
                imageWidths.Sort();

                var height = imageHeights[^1];
                var width = imageWidths[^1];

                var imageInfo = new SKImageInfo(width, height);
                using (var surface = SKSurface.Create(imageInfo))
                {
                    var canvas = surface.Canvas;

                    foreach (var imageFile in imageFiles)
                    {
                        var image = SKImage.FromEncodedData(imageFile.Path);
                        var bitmap = SKBitmap.FromImage(image);
                        canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
                        WaitOne();
                    }

                    using var img = surface.Snapshot();
                    using var data = img.Encode();
                    File.WriteAllBytes(destPath, data.ToArray());
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
