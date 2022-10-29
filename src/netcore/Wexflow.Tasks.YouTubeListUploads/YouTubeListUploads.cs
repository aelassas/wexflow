using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.YouTubeListUploads
{
    public class YouTubeListUploads:Task
    {
        public string User { get; }
        public string ApplicationName { get; }
        public string ClientSecrets { get; }

        public YouTubeListUploads(XElement xe, Workflow wf) : base(xe, wf)
        {
            User = GetSetting("user");
            ApplicationName = GetSetting("applicationName");
            ClientSecrets = GetSetting("clientSecrets");
        }

        public override TaskStatus Run()
        {
            Info("Listing uploads...");

            Status status = Status.Success;

            try
            {
                ListUploads().Wait();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while listing uploads: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private async System.Threading.Tasks.Task ListUploads()
        {
            UserCredential credential;
            using (var stream = new FileStream(ClientSecrets, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] {YouTubeService.Scope.YoutubeReadonly},
                    User,
                    CancellationToken.None,
                    new FileDataStore(GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", "YouTubeListUploads", DateTime.Now));

            var xdoc = new XDocument(new XElement("YouTubeListUploads"));
            var xchannels = new XElement("Channels");
            
            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                InfoFormat("Videos in list {0}", uploadsListId);

                var xchannel = new XElement("Channel", new XAttribute("id", uploadsListId));
                var xvideos = new XElement("Videos");

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                    foreach (var playlistItem in playlistItemsListResponse.Items)
                    {
                        // Print information about each video.
                        InfoFormat("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                        xvideos.Add(new XElement("Video", new XAttribute("id", playlistItem.Snippet.ResourceId.VideoId), new XAttribute("title", playlistItem.Snippet.Title)));
                    }

                    xchannel.Add(xvideos);

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }

                xchannels.Add(xchannel);
            }

            xdoc.Root.Add(xchannels);
            xdoc.Save(xmlPath);
            Files.Add(new FileInf(xmlPath, Id));
            InfoFormat("Results written in {0}", xmlPath);
        }

    }
}
