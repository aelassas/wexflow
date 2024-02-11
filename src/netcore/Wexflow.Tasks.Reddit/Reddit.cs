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

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();
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
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("Authentication failed: {0}", e.Message);
                    return new TaskStatus(Status.Error);
                }

                foreach (var file in files)
                {
                    try
                    {
                        var xdoc = XDocument.Load(file.Path);

                        // Self posts
                        foreach (var xPost in xdoc.XPathSelectElements("Reddit/SelfPosts/SelfPost"))
                        {
                            var subreddit = xPost.Attribute("subreddit")!.Value;
                            var title = xPost.Attribute("title")!.Value;
                            var text = xPost.Attribute("text")!.Value;

                            var sub = reddit.Subreddit(subreddit);
                            var selfPost = sub.SelfPost(title, text).Submit();

                            if (selfPost == null)
                            {
                                ErrorFormat("An error occured while sending the post '{0}'", title);
                                success = false;
                            }
                            else
                            {
                                InfoFormat("Post '{0}' sent. Id: {1}", title, selfPost.Id);

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                        }

                        // Link posts
                        foreach (var xLink in xdoc.XPathSelectElements("Reddit/LinkPosts/LinkPost"))
                        {
                            var subreddit = xLink.Attribute("subreddit")!.Value;
                            var title = xLink.Attribute("title")!.Value;
                            var url = xLink.Attribute("url")!.Value;

                            var sub = reddit.Subreddit(subreddit);
                            var linkPost = sub.LinkPost(title, url).Submit();

                            if (linkPost == null)
                            {
                                ErrorFormat("An error occured while sending the link post '{0}'", title);
                                success = false;
                            }
                            else
                            {
                                InfoFormat("Link post '{0}' sent. Id: {1}", title, linkPost.Id);

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while sending posts of the file {0}.", e, file.Path);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
                }
            }

            var tstatus = Status.Success;

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
