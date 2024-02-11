using System;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Tweetinvi;
using Tweetinvi.Models;
using Wexflow.Core;

namespace Wexflow.Tasks.Twitter
{
    public class Twitter : Task
    {
        public string ConsumerKey { get; }
        public string ConsumerSecret { get; }
        public string AccessToken { get; }
        public string AccessTokenSecret { get; }

        public Twitter(XElement xe, Workflow wf) : base(xe, wf)
        {
            ConsumerKey = GetSetting("consumerKey");
            ConsumerSecret = GetSetting("consumerSecret");
            AccessToken = GetSetting("accessToken");
            AccessTokenSecret = GetSetting("accessTokenSecret");
        }

        public override TaskStatus Run()
        {
            Info("Sending tweets...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                TwitterClient client;
                try
                {
                    TwitterCredentials credentials = new(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
                    client = new TwitterClient(credentials);
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
                finally
                {
                    WaitOne();
                }

                foreach (var file in files)
                {
                    try
                    {
                        var xdoc = XDocument.Load(file.Path);
                        foreach (var xTweet in xdoc.XPathSelectElements("Tweets/Tweet"))
                        {
                            var status = xTweet.Value;
                            //var tweet = Tweet.PublishTweet(status);
                            var tweetTask = client.Tweets.PublishTweetAsync(status);
                            tweetTask.Wait();
                            var tweet = tweetTask.Result;

                            if (tweet != null)
                            {
                                InfoFormat("Tweet '{0}' sent. Id: {1}", status, tweet.Id);

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                ErrorFormat("An error occured while sending the tweet '{0}'", status);
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
                        ErrorFormat("An error occured while sending the tweets of the file {0}.", e, file.Path);
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