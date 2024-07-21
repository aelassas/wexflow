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
    public class YouTubeListUploads : Task
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

            var status = Status.Success;

            try
            {
                ListUploads().Wait();
            }
            catch (ThreadInterruptedException)
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
            await using (FileStream stream = new(ClientSecrets, FileMode.Open, FileAccess.Read))
            {
#pragma warning disable CS0618 // Le type ou le membre est obsolète
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    [YouTubeService.Scope.YoutubeReadonly],
                    User,
                    CancellationToken.None,
                    new FileDataStore(GetType().ToString())
                );
#pragma warning restore CS0618 // Le type ou le membre est obsolète
            }

            YouTubeService youtubeService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"YouTubeListUploads_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

            XDocument xdoc = new(new XElement("YouTubeListUploads"));
            XElement xchannels = new("Channels");

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                InfoFormat("Videos in list {0}", uploadsListId);

                XElement xchannel = new("Channel", new XAttribute("id", uploadsListId));
                XElement xvideos = new("Videos");

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
                    WaitOne();
                }

                xchannels.Add(xchannel);
            }

            if (xdoc.Root == null)
            {
                throw new InvalidOperationException();
            }

            xdoc.Root.Add(xchannels);
            xdoc.Save(xmlPath);
            Files.Add(new FileInf(xmlPath, Id));
            InfoFormat("Results written in {0}", xmlPath);
        }
    }
}
