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
            var atLeastOneSuccess = false;

            bool success;
            try
            {
                success = UploadVideos(ref atLeastOneSuccess);
            }
            catch (ThreadInterruptedException)
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
                VimeoDotNet.VimeoClient vimeoApi = new(Token);
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
                                using (BinaryContent vfile = new(file.Path))
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
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while uploading the file {0}: {1}", file.Path, e.Message);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
                }
            }
            catch (ThreadInterruptedException)
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
