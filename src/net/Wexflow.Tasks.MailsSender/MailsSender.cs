using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Wexflow.Tasks.MailsSender
{
    public class MailsSender : Task
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public bool IsBodyHtml { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public MailsSender(XElement xe, Workflow wf) : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port"));
            EnableSsl = bool.Parse(GetSetting("enableSsl"));
            User = GetSetting("user");
            Password = GetSetting("password");
            IsBodyHtml = bool.Parse(GetSetting("isBodyHtml", "true"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Sending mails...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = SendMails(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = SendMails(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending Emails.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSuccess)
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

        private bool SendMails(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                FileInf[] attachments = SelectAttachments();

                foreach (FileInf mailFile in SelectFiles())
                {
                    var xdoc = XDocument.Load(mailFile.Path);
                    var xMails = xdoc.XPathSelectElements("Mails/Mail");

                    int count = 1;
                    foreach (XElement xMail in xMails)
                    {
                        Mail mail;
                        try
                        {
                            mail = Mail.Parse(this, xMail, attachments);
                        }
                        catch (ThreadAbortException)
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

                            if (!atLeastOneSuccess) atLeastOneSuccess = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while sending the mail {0}. Error message: {1}", count, e.Message);
                            success = false;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending mails.", e);
                success = false;
            }
            return success;
        }

        public string ParseVariables(string src)
        {
            //
            // Parse local variables.
            //
            var res = string.Empty;
            using (StringReader sr = new StringReader(src))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string pattern = @"{.*?}";
                    Match m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
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
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            var res2 = string.Empty;
            using (StringReader sr = new StringReader(res))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in Workflow.RestVariables)
                    {
                        if (variable != null)
                        {
                            line = line.Replace("$" + variable.Key, variable.Value);
                        }
                    }
                    sw.WriteLine(line);
                }
                res2 = sw.ToString();
            }

            return res2.Trim('\r', '\n');
        }

        private FileInf[] SelectAttachments()
        {
            var files = new List<FileInf>();
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
            return files.ToArray();
        }
    }
}
