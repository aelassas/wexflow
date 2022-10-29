using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.YouTube
{
    internal enum PrivacyStatus
    {
        Unlisted,
        Private,
        Public
    }

    public class YouTube : Task
    {
        public string User { get; }
        public string ApplicationName { get; }
        public string ClientSecrets { get; }

        public YouTube(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            User = GetSetting("user");
            ApplicationName = GetSetting("applicationName");
            ClientSecrets = GetSetting("clientSecrets");
        }

        public override TaskStatus Run()
        {
            Info("Uploading videos...");

            bool succeeded = true;
            bool atLeastOneSucceed = false;

            try
            {
                var files = SelectFiles();

                foreach (var file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);

                        foreach (var xvideo in xdoc.XPathSelectElements("/Videos/Video"))
                        {
                            string title = xvideo.Element("Title").Value;
                            string desc = xvideo.Element("Description").Value;
                            string[] tags = xvideo.Element("Tags").Value.Split(',');
                            string categoryId = xvideo.Element("CategoryId").Value;
                            PrivacyStatus ps = (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), xvideo.Element("PrivacyStatus").Value, true);
                            string filePath = xvideo.Element("FilePath").Value;

                            var succeededTask = UploadVideo(title, desc, tags, categoryId, ps, filePath);
                            succeededTask.Wait();
                            succeeded &= succeededTask.Result;

                            if (succeeded && !atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while uploading the file {0}: {1}", file.Path, e.Message);
                        succeeded = false;
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
                return new TaskStatus(Status.Error);
            }

            var status = Status.Success;

            if (!succeeded && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!succeeded)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private async System.Threading.Tasks.Task<bool> UploadVideo(string title, string desc, string[] tags, string categoryId, PrivacyStatus ps, string filePath)
        {
            try
            {
                InfoFormat("Uploading the video file {0} to YouTube started...", filePath);
                Info("Authentication started...");
                UserCredential credential;
                using (var stream = new FileStream(ClientSecrets, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        // This OAuth 2.0 access scope allows an application to upload files to the
                        // authenticated user's YouTube channel, but doesn't allow other types of access.
                        new[] { YouTubeService.Scope.YoutubeUpload },
                        User,
                        CancellationToken.None
                        );
                }
                Info("Authentication succeeded.");

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var video = new Video
                {
                    Snippet = new VideoSnippet()
                };
                video.Snippet.Title = title;
                video.Snippet.Description = desc;
                video.Snippet.Tags = tags;
                video.Snippet.CategoryId = categoryId; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
                video.Status = new VideoStatus
                {
                    PrivacyStatus = ps.ToString().ToLower() // "unlisted" or "private" or "public"
                };

                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                    videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

                    var res = videosInsertRequest.Upload();

                    if (res.Exception != null)
                    {
                        ErrorFormat("An error occured while uploading the file {0}: {1}", filePath, res.Exception.Message);
                        return false;
                    }
                }

                InfoFormat("Uploading the video file {0} to YouTube succeeded.", filePath);
                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while uploading the video file {0}: {1}", filePath, e.Message);
                return false;
            }
        }

        private void VideosInsertRequest_ResponseReceived(Video video)
        {
            InfoFormat("The video '{0}' was successfully uploaded. Id: '{1}' ", video.Snippet.Title, video.Id);
        }

    }
}
