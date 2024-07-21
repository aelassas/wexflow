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

            var status = Status.Success;

            RedditClient reddit;
            try
            {
                reddit = new RedditClient(AppId, RefreshToken);
                InfoFormat("Username: {0}", reddit.Account.Me.Name);
                InfoFormat("Cake Day: {0}", reddit.Account.Me.Created.ToString("D"));
                Info("Authentication succeeded.");
            }
            catch (ThreadInterruptedException)
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

                XDocument xdoc = new(new XElement("Posts"));

                foreach (var post in posts)
                {
                    XElement xpost = new("Post", new XAttribute("id", SecurityElement.Escape(post.Id) ?? throw new InvalidOperationException()),
                        new XAttribute("subreddit", SecurityElement.Escape(post.Subreddit) ?? throw new InvalidOperationException()),
                        new XAttribute("title", SecurityElement.Escape(post.Title) ?? throw new InvalidOperationException()),
                        new XAttribute("upvotes", post.UpVotes),
                        new XAttribute("downvotes", post.DownVotes));
                    if (xdoc.Root == null)
                    {
                        throw new InvalidOperationException();
                    }

                    xdoc.Root.Add(xpost);
                }

                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"RedditListPosts_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("Post history written in {0}", xmlPath);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while retrieving post history: {0}", e.Message);
                status = Status.Error;
            }
            finally
            {
                WaitOne();
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
