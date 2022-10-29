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

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                success = UploadVideos(ref atLeastOneSuccess);
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
                        XDocument xdoc = XDocument.Load(file.Path);

                        foreach (var xvideo in xdoc.XPathSelectElements("/Videos/Video"))
                        {
                            string title = xvideo.Element("Title").Value;
                            string desc = xvideo.Element("Description").Value;
                            string filePath = xvideo.Element("FilePath").Value;

                            try
                            {
                                using (var vfile = new BinaryContent(file.Path))
                                {
                                    var uploadTask = vimeoApi.UploadEntireFileAsync(vfile);
                                    uploadTask.Wait();
                                    var videoId = uploadTask.Result.ClipId;
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
