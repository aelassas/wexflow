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
        public string Token { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

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

            bool success;
            var atLeastOneSuccess = false;

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
            return new TaskStatus(status);
        }

        private bool UploadVideos(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var files = SelectFiles();
                var vimeoApi = new VimeoDotNet.VimeoClient(Token);

                foreach (var file in files)
                {
                    try
                    {
                        var xdoc = XDocument.Load(file.Path);

                        foreach (var xvideo in xdoc.XPathSelectElements("/Videos/Video"))
                        {
                            //var title = xvideo.Element("Title").Value;
                            //var desc = xvideo.Element("Description").Value;
                            var filePath = xvideo.Element("FilePath").Value;

                            try
                            {
                                using (var vfile = new BinaryContent(file.Path))
                                {
                                    var uploadTask = vimeoApi.UploadEntireFileAsync(vfile);
                                    uploadTask.Wait();
                                    var videoId = uploadTask.Result.ClipId;
                                    InfoFormat("Video {0} uploaded to Vimeo. VideoId: {1}", filePath, videoId);
                                }

                                if (success && !atLeastOneSuccess)
                                {
                                    atLeastOneSuccess = true;
                                }
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
