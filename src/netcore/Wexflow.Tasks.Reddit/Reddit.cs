using Reddit;
using System;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.Reddit
{
    public class Reddit : Task
    {
        public string AppId { get; }
        public string RefreshToken { get; }

        public Reddit(XElement xe, Workflow wf) : base(xe, wf)
        {
            AppId = GetSetting("appId");
            RefreshToken = GetSetting("refreshToken");
        }

        public override TaskStatus Run()
        {
            Info("Posting on Reddit...");

            bool success = true;
            bool atLeastOneSucceed = false;

            FileInf[] files = SelectFiles();
            if (files.Length > 0)
            {
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

                foreach (FileInf file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);

                        // Self posts
                        foreach (XElement xPost in xdoc.XPathSelectElements("Reddit/SelfPosts/SelfPost"))
                        {
                            string subreddit = xPost.Attribute("subreddit").Value;
                            string title = xPost.Attribute("title").Value;
                            string text = xPost.Attribute("text").Value;

                            global::Reddit.Controllers.Subreddit sub = reddit.Subreddit(subreddit);
                            global::Reddit.Controllers.SelfPost selfPost = sub.SelfPost(title, text).Submit();

                            if (selfPost == null)
                            {
                                ErrorFormat("An error occured while sending the post '{0}'", title);
                                success = false;
                            }
                            else
                            {
                                InfoFormat("Post '{0}' sent. Id: {1}", title, selfPost.Id);

                                if (!atLeastOneSucceed) atLeastOneSucceed = true;
                            }
                        }

                        // Link posts
                        foreach (XElement xLink in xdoc.XPathSelectElements("Reddit/LinkPosts/LinkPost"))
                        {
                            string subreddit = xLink.Attribute("subreddit").Value;
                            string title = xLink.Attribute("title").Value;
                            string url = xLink.Attribute("url").Value;

                            global::Reddit.Controllers.Subreddit sub = reddit.Subreddit(subreddit);
                            global::Reddit.Controllers.LinkPost linkPost = sub.LinkPost(title, url).Submit();

                            if (linkPost == null)
                            {
                                ErrorFormat("An error occured while sending the link post '{0}'", title);
                                success = false;
                            }
                            else
                            {
                                InfoFormat("Link post '{0}' sent. Id: {1}", title, linkPost.Id);

                                if (!atLeastOneSucceed) atLeastOneSucceed = true;

                            }
                        }

                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while sending posts of the file {0}.", e, file.Path);
                        success = false;
                    }
                }
            }

            Status tstatus = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                tstatus = Status.Warning;
            }
            else if (!success)
            {
                tstatus = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(tstatus);
        }
    }
}
