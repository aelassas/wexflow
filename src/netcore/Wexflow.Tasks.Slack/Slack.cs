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

            var files = SelectFiles();

            if (files.Length > 0)
            {
                ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
                SlackSocketClient client = new SlackSocketClient(Token);
                client.Connect((connected) => {
                    // This is called once the client has emitted the RTM start command
                    clientReady.Set();
                }, () => {
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
                        var xdoc = XDocument.Load(file.Path);
                        foreach (XElement xMessage in xdoc.XPathSelectElements("Messages/Message"))
                        {
                            var username = xMessage.Element("User").Value;
                            var text = xMessage.Element("Text").Value;

                            var user = client.Users.Find(x => x.name.Equals(username));
                            var dmchannel = client.DirectMessages.Find(x => x.user.Equals(user.id));
                            client.PostMessage((mr) => Info("Message '" + text + "' sent to " + dmchannel.id + "."), dmchannel.id, text);

                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
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
