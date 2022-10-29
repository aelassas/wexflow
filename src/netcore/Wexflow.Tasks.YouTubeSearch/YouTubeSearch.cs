using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Wexflow.Core;

namespace Wexflow.Tasks.YouTubeSearch
{
    public class YouTubeSearch : Task
    {
        public string ApplicationName { get; }
        public string ApiKey { get; }
        public string Keyword { get; }
        public int MaxResults { get; }

        public YouTubeSearch(XElement xe, Workflow wf) : base(xe, wf)
        {
            ApplicationName = GetSetting("applicationName");
            ApiKey = GetSetting("apiKey");
            Keyword = GetSetting("keyword");
            MaxResults = int.Parse(GetSetting("maxResults", "50"));
        }

        public override TaskStatus Run()
        {
            Info("Searching for content...");
            Status status = Status.Success;
            try
            {
                Search().Wait();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                status = Status.Error;
                ErrorFormat("An error occured while searching for content: {1}", e.Message);
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private async System.Threading.Tasks.Task Search()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ApiKey,
                ApplicationName = ApplicationName
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Keyword;
            searchListRequest.MaxResults = MaxResults;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", "YouTubeSearch", DateTime.Now));

            var xdoc = new XDocument(new XElement("YouTubeSearch"));
            var xvideos = new XElement("Videos");
            var xchannels = new XElement("Channels");
            var xplaylists = new XElement("Playlists");

            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        xvideos.Add(new XElement("Video", new XAttribute("id", searchResult.Id.VideoId), new XAttribute("title", searchResult.Snippet.Title)));
                        break;

                    case "youtube#channel":
                        channels.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        xchannels.Add(new XElement("Channel", new XAttribute("id", searchResult.Id.ChannelId), new XAttribute("title", searchResult.Snippet.Title)));
                        break;

                    case "youtube#playlist":
                        playlists.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        xplaylists.Add(new XElement("Playlist", new XAttribute("id", searchResult.Id.PlaylistId), new XAttribute("title", searchResult.Snippet.Title)));
                        break;
                }
            }

            InfoFormat("Videos:\n{0}\n", string.Join("\n", videos));
            InfoFormat("Channels:\n{0}\n", string.Join("\n", channels));
            InfoFormat("Playlists:\n{0}\n", string.Join("\n", playlists));

            xdoc.Root.Add(xvideos);
            xdoc.Root.Add(xchannels);
            xdoc.Root.Add(xplaylists);
            xdoc.Save(xmlPath);
            Files.Add(new FileInf(xmlPath, Id));
            InfoFormat("Search results written in {0}", xmlPath);
        }
    }
}
