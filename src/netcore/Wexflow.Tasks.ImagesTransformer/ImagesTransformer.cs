using SkiaSharp;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesTransformer
{
    public enum ImgFormat
    {
        Bmp,
        //Emf,
        //Exif,
        Gif,
        Icon,
        Jpeg,
        Png,
        //Tiff,
        //Wmf
    }

    [SupportedOSPlatform("windows")]
    public class ImagesTransformer : Task
    {
        public string OutputFilePattern { get; }
        public ImgFormat OutputFormat { get; }

        public ImagesTransformer(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            OutputFilePattern = GetSetting("outputFilePattern");
            OutputFormat = (ImgFormat)Enum.Parse(typeof(ImgFormat), GetSetting("outputFormat"), true);
        }

        public override TaskStatus Run()
        {
            Info("Transforming images...");

            var success = true;
            var atLeastOneSucceed = false;

            foreach (var file in SelectFiles())
            {
                try
                {
                    var destFilePath = Path.Combine(Workflow.WorkflowTempFolder,
                        OutputFilePattern.Replace("$fileNameWithoutExtension", Path.GetFileNameWithoutExtension(file.FileName)).Replace("$fileName", file.FileName));

                    using var skBitmap = SKBitmap.Decode(file.Path);
                    {
                        switch (OutputFormat)
                        {
                            case ImgFormat.Bmp:
                                SaveImage(skBitmap, SKEncodedImageFormat.Bmp, destFilePath);
                                break;
                            //case ImgFormat.Emf:
                            //    img.Save(destFilePath, ImageFormat.Emf);
                            //    break;
                            //case ImgFormat.Exif:
                            //    img.Save(destFilePath, ImageFormat.Exif);
                            //    break;
                            case ImgFormat.Gif:
                                SaveImage(skBitmap, SKEncodedImageFormat.Gif, destFilePath);
                                break;
                            case ImgFormat.Icon:
                                SaveImage(skBitmap, SKEncodedImageFormat.Ico, destFilePath);
                                break;
                            case ImgFormat.Jpeg:
                                SaveImage(skBitmap, SKEncodedImageFormat.Jpeg, destFilePath);
                                break;
                            case ImgFormat.Png:
                                SaveImage(skBitmap, SKEncodedImageFormat.Png, destFilePath);
                                break;
                            //case ImgFormat.Tiff:
                            //    img.Save(destFilePath, ImageFormat.Tiff);
                            //    break;
                            //case ImgFormat.Wmf:
                            //    SaveImage(skBitmap, SKEncodedImageFormat.Jpeg, destFilePath);
                            //    break;
                            default:
                                break;
                        }
                    }
                    Files.Add(new FileInf(destFilePath, Id));
                    InfoFormat("Image {0} transformed to {1}", file.Path, destFilePath);

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while transforming the image {0}. Error: {1}", file.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
                }
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private static void SaveImage(SKBitmap skBitmap, SKEncodedImageFormat sKEncodedImageFormat, string destFilePath)
        {
            using var data = skBitmap.Encode(sKEncodedImageFormat, 80);
            File.WriteAllBytes(destFilePath, data.ToArray());
        }
    }
}
