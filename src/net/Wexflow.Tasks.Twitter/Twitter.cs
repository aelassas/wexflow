using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading;
using Tweetinvi;

namespace Wexflow.Tasks.Twitter
{
    public class Twitter : Task
    {
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public string AccessToken { get; private set; }
        public string AccessTokenSecret { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Twitter(XElement xe, Workflow wf) : base(xe, wf)
        {
            ConsumerKey = GetSetting("consumerKey");
            ConsumerSecret = GetSetting("consumerSecret");
            AccessToken = GetSetting("accessToken");
            AccessTokenSecret = GetSetting("accessTokenSecret");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Sending tweets...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = SendTweets(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = SendTweets(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending tweets.", e);
                success = false;
            }

            var tstatus = Status.Success;

            if (!success && atLeastOneSuccess)
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

        private bool SendTweets(ref bool atLeastOneSuccess)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                try
                {
                    TweetinviConfig.ApplicationSettings.HttpRequestTimeout = 20000;
                    TweetinviConfig.CurrentThreadSettings.InitialiseFrom(TweetinviConfig.ApplicationSettings);

                    Auth.SetUserCredentials(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
                    var authenticatedUser = User.GetAuthenticatedUser();

                    if (authenticatedUser == null) // Something went wrong but we don't know what
                    {
                        // We can get the latest exception received by Tweetinvi
                        var latestException = ExceptionHandler.GetLastException();
                        ErrorFormat("The following error occured : '{0}'", latestException.TwitterDescription);
                        Error("Authentication failed.");
                        success = false;
                        return success;
                    }
                    Info("Authentication succeeded.");
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("Authentication failed: {0}", e.Message);
                    success = false;
                    return success;
                }

                foreach (FileInf file in files)
                {
                    try
                    {
                        var xdoc = XDocument.Load(file.Path);
                        foreach (XElement xTweet in xdoc.XPathSelectElements("Tweets/Tweet"))
                        {
                            var status = xTweet.Value;
                            var tweet = Tweet.PublishTweet(status);
                            if (tweet != null)
                            {
                                InfoFormat("Tweet '{0}' sent. Id: {1}", status, tweet.Id);

                                if (!atLeastOneSuccess) atLeastOneSuccess = true;
                            }
                            else
                            {
                                ErrorFormat("An error occured while sending the tweet '{0}'", status);
                                success = false;
                            }
                        }

                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while sending the tweets of the file {0}.", e, file.Path);
                        success = false;
                    }
                }
            }
            return success;
        }

    }
}