using Reddit;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.RedditListComments
{
    public class RedditListComments : Task
    {
        public string AppId { get; }
        public string RefreshToken { get; }
        public int MaxResults { get; }

        public RedditListComments(XElement xe, Workflow wf) : base(xe, wf)
        {
            AppId = GetSetting("appId");
            RefreshToken = GetSetting("refreshToken");
            MaxResults = int.Parse(GetSetting("maxResults", "50"));
        }

        public override TaskStatus Run()
        {
            Info("Retrieving comment history...");

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
                // Retrieve the authenticated user's recent comment history.
                var comments = reddit.Account.Me.GetCommentHistory(sort: "new", limit: MaxResults, show: "all");

                XDocument xdoc = new(new XElement("Comments"));

                foreach (var comment in comments)
                {
                    XElement xcomment = new("Comment", new XAttribute("id", SecurityElement.Escape(comment.Id) ?? throw new InvalidOperationException()),
                        new XAttribute("subreddit", SecurityElement.Escape(comment.Subreddit) ?? throw new InvalidOperationException()),
                        new XAttribute("author", SecurityElement.Escape(comment.Author) ?? throw new InvalidOperationException()),
                        new XAttribute("upvotes", comment.UpVotes),
                        new XAttribute("downvotes", comment.DownVotes), new XCData(comment.BodyHTML));
                    if (xdoc.Root == null)
                    {
                        throw new InvalidOperationException();
                    }

                    xdoc.Root.Add(xcomment);
                }

                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"RedditListComments_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("Comment history written in {0}", xmlPath);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while retrieving comment history: {0}", e.Message);
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
