using Reddit;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.RedditListPosts
{
    public class RedditListPosts : Task
    {
        public string AppId { get; }
        public string RefreshToken { get; }
        public int MaxResults { get; }

        public RedditListPosts(XElement xe, Workflow wf) : base(xe, wf)
        {
            AppId = GetSetting("appId");
            RefreshToken = GetSetting("refreshToken");
            MaxResults = int.Parse(GetSetting("maxResults", "50"));
        }

        public override TaskStatus Run()
        {
            Info("Retrieving post history...");

            Status status = Status.Success;

            RedditClient reddit;
            try
            {
                reddit = new RedditClient(AppId, RefreshToken);
                InfoFormat("Username: {0}", reddit.Account.Me.Name);
                InfoFormat("Cake Day: {0}", reddit.Account.Me.Created.ToString("D"));
                Info("Authentication succeeded.");
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("Authentication failed: {0}", e.Message);
                return new TaskStatus(Status.Error);
            }


            try
            {
                // Retrieve the authenticated user's recent post history.
                // Change "new" to "newForced" if you don't want older stickied profile posts to appear first.
                var posts = reddit.Account.Me.GetPostHistory(sort: "new", limit: MaxResults, show: "all");

                var xdoc = new XDocument(new XElement("Posts"));

                foreach(var post in posts)
                {
                    var xpost = new XElement("Post", new XAttribute("id", SecurityElement.Escape(post.Id)), new XAttribute("subreddit", SecurityElement.Escape(post.Subreddit)), new XAttribute("title", SecurityElement.Escape(post.Title)), new XAttribute("upvotes", post.UpVotes), new XAttribute("downvotes", post.DownVotes));
                    xdoc.Root.Add(xpost);
                }

                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", "RedditListPosts", DateTime.Now));
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("Post history written in {0}", xmlPath);

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while retrieving post history: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
