using SlackAPI;
using System;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.Slack
{
    public class Slack : Task
    {
        public string Token { get; }

        public Slack(XElement xe, Workflow wf) : base(xe, wf)
        {
            Token = GetSetting("token");
        }

        public override TaskStatus Run()
        {
            Info("Sending slack messages...");

            bool success = true;
            bool atLeastOneSucceed = false;

            FileInf[] files = SelectFiles();

            if (files.Length > 0)
            {
                ManualResetEventSlim clientReady = new(false);
                SlackSocketClient client = new(Token);
                client.Connect((connected) =>
                {
                    // This is called once the client has emitted the RTM start command
                    clientReady.Set();
                }, () =>
                {
                    // This is called once the RTM client has connected to the end point
                });
                client.OnMessageReceived += (message) =>
                {
                    // Handle each message as you receive them
                };
                clientReady.Wait();
                client.GetUserList((ulr) => { Info("Got users."); });

                foreach (FileInf file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);
                        foreach (XElement xMessage in xdoc.XPathSelectElements("Messages/Message"))
                        {
                            string username = xMessage.Element("User").Value;
                            string text = xMessage.Element("Text").Value;

                            if (client.Users != null)
                            {
                                User user = client.Users.Find(x => x.name.Equals(username));
                                DirectMessageConversation dmchannel = client.DirectMessages.Find(x => x.user.Equals(user.id));
                                client.PostMessage((mr) => Info("Message '" + text + "' sent to " + dmchannel.id + "."), dmchannel.id, text);

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
                        ErrorFormat("An error occured while sending the messages of the file {0}.", e, file.Path);
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
