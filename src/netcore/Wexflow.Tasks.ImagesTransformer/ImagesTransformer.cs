using System;
using Wexflow.Core;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Wexflow.Tasks.ImagesTransformer
{
    public enum ImgFormat
    {
        Bmp,
        Emf,
        Exif,
        Gif,
        Icon,
        Jpeg,
        Png,
        Tiff,
        Wmf
    }

    public class ImagesTransformer:Task
    {
        public string OutputFilePattern { get; private set; }
        public ImgFormat OutputFormat { get; private set; }

        public ImagesTransformer(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            OutputFilePattern = GetSetting("outputFilePattern");
            OutputFormat = (ImgFormat)Enum.Parse(typeof(ImgFormat), GetSetting("outputFormat"), true);
        }

        public override TaskStatus Run()
        {
            Info("Transforming images...");

            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (FileInf file in SelectFiles())
            {
                try
                {
                    var destFilePath = Path.Combine(Workflow.WorkflowTempFolder,
                        OutputFilePattern.Replace("$fileNameWithoutExtension", Path.GetFileNameWithoutExtension(file.FileName)).Replace("$fileName", file.FileName));

                    using (Image img = Image.FromFile(file.Path))
                    {
                        switch (OutputFormat)
                        {
                            case ImgFormat.Bmp:
                                img.Save(destFilePath, ImageFormat.Bmp);
                                break;
                            case ImgFormat.Emf:
                                img.Save(destFilePath, ImageFormat.Emf);
                                break;
                            case ImgFormat.Exif:
                                img.Save(destFilePath, ImageFormat.Exif);
                                break;
                            case ImgFormat.Gif:
                                img.Save(destFilePath, ImageFormat.Gif);
                                break;
                            case ImgFormat.Icon:
                                img.Save(destFilePath, ImageFormat.Icon);
                                break;
                            case ImgFormat.Jpeg:
                                img.Save(destFilePath, ImageFormat.Jpeg);
                                break;
                            case ImgFormat.Png:
                                img.Save(destFilePath, ImageFormat.Png);
                                break;
                            case ImgFormat.Tiff:
                                img.Save(destFilePath, ImageFormat.Tiff);
                                break;
                            case ImgFormat.Wmf:
                                img.Save(destFilePath, ImageFormat.Wmf);
                                break;
                        }
                    }
                    Files.Add(new FileInf(destFilePath, Id));
                    InfoFormat("Image {0} transformed to {1}", file.Path, destFilePath);
                    
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while transforming the image {0}. Error: {1}", file.Path, e.Message);
                    success = false;
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
    }
}
