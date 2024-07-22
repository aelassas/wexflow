using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.MailsReceiver
{
    public enum Protocol
    {
        Pop3,
        Imap
    }

    public class MailsReceiver : Task
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string User { get; }
        public string Password { get; }
        public int MessageCount { get; }
        public bool DeleteMessages { get; }
        public Protocol Protocol { get; }

        public MailsReceiver(XElement xe, Workflow wf) : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port", "993"));
            EnableSsl = bool.Parse(GetSetting("enableSsl", "true"));
            User = GetSetting("user");
            Password = GetSetting("password");
            MessageCount = int.Parse(GetSetting("messageCount"));
            DeleteMessages = bool.Parse(GetSetting("deleteMessages", "false"));
            Protocol = (Protocol)Enum.Parse(typeof(Protocol), GetSetting("protocol", "pop3"), true);
        }

        public override TaskStatus Run()
        {
            Info("Receiving mails...");

            var success = true;
            var atLeastOneSucceed = false;

            try
            {
                switch (Protocol)
                {
                    case Protocol.Imap:
                        using (ImapClient client = new())
                        {
                            client.Connect(Host, Port, EnableSsl);
                            client.Authenticate(User, Password);
                            _ = client.Inbox.Open(FolderAccess.ReadOnly);

                            var uids = client.Inbox.Search(SearchQuery.All);

                            var count = uids.Count;

                            for (var i = Math.Min(MessageCount, count); i > 0; i--)
                            {
                                var message = client.Inbox.GetMessage(uids[i]);
                                var messageFileName = $"message_{i}_{message.Date:yyyy-MM-dd-HH-mm-ss-fff}";
                                var messagePath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + ".eml");
                                message.WriteTo(messagePath);
                                Files.Add(new FileInf(messagePath, Id));
                                InfoFormat("Message {0} received. Path: {1}", i, messagePath);

                                // save attachments
                                var j = 0;
                                var attachments = message.Attachments.ToList();
                                foreach (var attachment in attachments)
                                {
                                    if (attachment.IsAttachment)
                                    {
                                        var attachmentPath = Path.Combine(Workflow.WorkflowTempFolder, $"{messageFileName}_{(attachment is MessagePart ? ++j + ".eml" : ((MimePart)attachment).FileName)}");

                                        if (attachment is not MessagePart part)
                                        {
                                            using var stream = File.Create(attachmentPath);
                                            ((MimePart)attachment).Content.DecodeTo(stream);
                                        }
                                        else
                                        {
                                            part.WriteTo(attachmentPath);
                                        }

                                        Files.Add(new FileInf(attachmentPath, Id));
                                        InfoFormat("Attachment {0} of mail {1} received. Path: {2}", attachment is MessagePart ? j + ".eml" : ((MimePart)attachment).FileName, i, attachmentPath);
                                    }
                                }

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }

                                WaitOne();
                            }

                            client.Disconnect(true);
                        }
                        break;
                    case Protocol.Pop3:
                        using (Pop3Client client = new())
                        {
                            client.Connect(Host, Port, EnableSsl);
                            _ = client.AuthenticationMechanisms.Remove("XOAUTH2");
                            client.Authenticate(User, Password);

                            var count = client.GetMessageCount();

                            // We want to download messages
                            // Messages are numbered in the interval: [1, messageCount]
                            // Ergo: message numbers are 1-based.
                            // Most servers give the latest message the highest number
                            for (var i = Math.Min(MessageCount, count); i > 0; i--)
                            {
                                var message = client.GetMessage(i);
                                var messageFileName = $"message_{i}_{message.Date:yyyy-MM-dd-HH-mm-ss-fff}";
                                var messagePath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + ".eml");
                                message.WriteTo(messagePath);
                                Files.Add(new FileInf(messagePath, Id));
                                InfoFormat("Message {0} received. Path: {1}", i, messagePath);

                                // save attachments
                                var attachments = message.Attachments.ToList();
                                foreach (var attachment in attachments)
                                {
                                    if (attachment.IsAttachment)
                                    {
                                        var attachmentPath = Path.Combine(Workflow.WorkflowTempFolder, $"{messageFileName}_{attachment.ContentId}");
                                        attachment.WriteTo(attachmentPath);
                                        Files.Add(new FileInf(attachmentPath, Id));
                                        InfoFormat("Attachment {0} of mail {1} received. Path: {2}", attachment.ContentId, i, attachmentPath);
                                    }
                                }

                                if (DeleteMessages)
                                {
                                    client.DeleteMessage(i);
                                }

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }

                            client.Disconnect(true);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while receiving mails.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
