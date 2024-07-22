using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ImagesTransformer
{
    public class ImagesTransformer : Task
    {
        public string OutputFilePattern { get; }
        public ImgFormat OutputFormat { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public ImagesTransformer(XElement xe, Workflow wf) : base(xe, wf)
        {
            OutputFilePattern = GetSetting("outputFilePattern");
            OutputFormat = (ImgFormat)Enum.Parse(typeof(ImgFormat), GetSetting("outputFormat"), true);
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Transforming images...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = Transform(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = Transform(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while transforming images.", e);
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
            return new TaskStatus(status, false);
        }

        private bool Transform(ref bool atLeastOneSuccess)
        {
            var success = true;
            foreach (var file in SelectFiles())
            {
                try
                {
                    var destFilePath = Path.Combine(Workflow.WorkflowTempFolder,
                        OutputFilePattern.Replace("$fileNameWithoutExtension", Path.GetFileNameWithoutExtension(file.FileName)).Replace("$fileName", file.FileName));

                    using (var img = Image.FromFile(file.Path))
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
                            default:
                                break;
                        }
                    }
                    Files.Add(new FileInf(destFilePath, Id));
                    InfoFormat("Image {0} transformed to {1}", file.Path, destFilePath);

                    if (!atLeastOneSuccess)
                    {
                        atLeastOneSuccess = true;
                    }
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
            return success;
        }
    }
}
