using System;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using VimeoDotNet.Net;
using Wexflow.Core;

namespace Wexflow.Tasks.Vimeo
{
    public class Vimeo : Task
    {
        public string Token { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Vimeo(XElement xe, Workflow wf) : base(xe, wf)
        {
            Token = GetSetting("token");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Uploading videos...");

            bool success = true;
            bool atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = UploadVideos(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = UploadVideos(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while uploading videos.", e);
                success = false;
            }

            Status status = Status.Success;

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

        private bool UploadVideos(ref bool atLeastOneSuccess)
        {
            bool success = true;
            try
            {
                FileInf[] files = SelectFiles();
                VimeoDotNet.VimeoClient vimeoApi = new VimeoDotNet.VimeoClient(Token);

                foreach (FileInf file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);

                        foreach (XElement xvideo in xdoc.XPathSelectElements("/Videos/Video"))
                        {
                            string title = xvideo.Element("Title").Value;
                            string desc = xvideo.Element("Description").Value;
                            string filePath = xvideo.Element("FilePath").Value;

                            try
                            {
                                using (BinaryContent vfile = new BinaryContent(file.Path))
                                {
                                    System.Threading.Tasks.Task<IUploadRequest> uploadTask = vimeoApi.UploadEntireFileAsync(vfile);
                                    uploadTask.Wait();
                                    long? videoId = uploadTask.Result.ClipId;
                                    InfoFormat("Video {0} uploaded to Vimeo. VideoId: {1}", filePath, videoId);
                                }

                                if (success && !atLeastOneSuccess) atLeastOneSuccess = true;
                            }
                            catch (Exception e)
                            {
                                ErrorFormat("An error occured while uploading the file {0}: {1}", filePath, e.Message);
                                success = false;
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while uploading the file {0}: {1}", file.Path, e.Message);
                        success = false;
                    }
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while uploading videos: {0}", e.Message);
                success = false;
            }
            return success;
        }

    }
}
