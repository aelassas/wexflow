using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.MailsSender
{
    public class MailsSender : Task
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string User { get; }
        public string Password { get; }
        public bool IsBodyHtml { get; }

        public MailsSender(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port"));
            EnableSsl = bool.Parse(GetSetting("enableSsl"));
            User = GetSetting("user");
            Password = GetSetting("password");
            IsBodyHtml = bool.Parse(GetSetting("isBodyHtml", "true"));
        }

        public override TaskStatus Run()
        {
            Info("Sending mails...");

            var success = true;
            var atLeastOneSucceed = false;

            try
            {
                var attachments = SelectAttachments();

                foreach (var mailFile in SelectFiles())
                {
                    var xdoc = XDocument.Load(mailFile.Path);
                    var xMails = xdoc.XPathSelectElements("Mails/Mail");

                    var count = 1;
                    foreach (var xMail in xMails)
                    {
                        Mail mail;
                        try
                        {
                            mail = Mail.Parse(this, xMail, attachments);
                        }
                        catch (ThreadInterruptedException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while parsing the mail {0}. Please check the XML configuration according to the documentation. Error: {1}", count, e.Message);
                            success = false;
                            count++;
                            continue;
                        }

                        try
                        {
                            mail.Send(Host, Port, EnableSsl, User, Password, IsBodyHtml);
                            InfoFormat("Mail {0} sent.", count);
                            count++;

                            if (!atLeastOneSucceed)
                            {
                                atLeastOneSucceed = true;
                            }
                        }
                        catch (ThreadInterruptedException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while sending the mail {0}. Error message: {1}", count, e.Message);
                            success = false;
                        }
                        finally
                        {
                            WaitOne();
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
                ErrorFormat("An error occured while sending mails.", e);
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

        public string ParseVariables(string src)
        {
            //
            // Parse local variables.
            //
            string res;
            using (StringReader sr = new(src))
            using (StringWriter sw = new())
            {
                while (sr.ReadLine() is { } line)
                {
                    var pattern = "{.*?}";
                    var m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        if (m.Value.StartsWith("{date:"))
                        {
                            var replaceValue = DateTime.Now.ToString(m.Value.Remove(m.Value.Length - 1).Remove(0, 6));
                            line = Regex.Replace(line, pattern, replaceValue);
                        }
                    }
                    foreach (var variable in Workflow.LocalVariables)
                    {
                        line = line.Replace($"${variable.Key}", variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            string res2;
            using (StringReader sr = new(res))
            using (StringWriter sw = new())
            {
                while (sr.ReadLine() is { } line)
                {
                    foreach (var variable in Workflow.RestVariables)
                    {
                        if (variable != null)
                        {
                            line = line.Replace($"${variable.Key}", variable.Value);
                        }
                    }
                    sw.WriteLine(line);
                }
                res2 = sw.ToString();
            }

            return res2.Trim('\r', '\n');
        }

        public FileInf[] SelectAttachments()
        {
            List<FileInf> files = [];
            foreach (var xSelectFile in GetXSettings("selectAttachments"))
            {
                var xTaskId = xSelectFile.Attribute("value");
                if (xTaskId != null)
                {
                    var taskId = int.Parse(xTaskId.Value);

                    var qf = QueryFiles(Workflow.FilesPerTask[taskId], xSelectFile).ToArray();

                    files.AddRange(qf);
                }
                else
                {
                    var qf = (from lf in Workflow.FilesPerTask.Values
                              from f in QueryFiles(lf, xSelectFile)
                              select f).Distinct().ToArray();

                    files.AddRange(qf);
                }
            }
            return [.. files];
        }
    }
}
