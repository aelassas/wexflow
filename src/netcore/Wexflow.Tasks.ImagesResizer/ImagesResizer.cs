using SkiaSharp;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesResizer
{
    [SupportedOSPlatform("windows")]
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
            var status = Status.Success;
            var succeeded = true;
            var atLeastOneSuccess = false;

            try
            {
                var images = SelectFiles();
                foreach (var image in images)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, image.FileName);
                    succeeded &= Resize(image.Path, destPath);
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
                using var skBitmap = SKBitmap.Decode(srcPath);
                using var scaledBitmap = skBitmap.Resize(new SKImageInfo(Width, Height), SKFilterQuality.Medium);
                using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                using var codec = SKCodec.Create(srcPath);
                using var data = scaledImage.Encode(codec.EncodedFormat, 80);
                File.WriteAllBytes(destPath, data.ToArray());

                Files.Add(new FileInf(destPath, Id));
                InfoFormat("The image {0} was resized to {1}x{2} -> {3}", srcPath, Width, Height, destPath);
                return true;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while resizing the image {0}: {1}", srcPath, e.Message);
                return false;
            }
        }
    }
}
