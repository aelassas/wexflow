using System;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Wexflow.Tasks.Sha256
{
    public class Sha256 : Task
    {
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Sha256(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Generating SHA-256 hashes...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = GenerateSha256(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = GenerateSha256(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while generating SHA-256.", e);
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

        private bool GenerateSha256(ref bool atLeastOneSuccess)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("SHA256_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        var sha1 = GetSha1(file.Path);
                        if (xdoc.Root != null)
                        {
                            xdoc.Root.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("sha256", sha1)));
                        }
                        InfoFormat("SHA-256 hash of the file {0} is {1}", file.Path, sha1);

                        if (!atLeastOneSuccess) atLeastOneSuccess = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the SHA-256 hash of the file {0}", e, file.Path);
                        success = false;
                    }
                }
                xdoc.Save(md5Path);
                Files.Add(new FileInf(md5Path, Id));
            }
            return success;
        }

        private string GetSha1(string filePath)
        {
            var sb = new StringBuilder();
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    var bytes = sha256.ComputeHash(stream);

                    foreach (byte bt in bytes)
                    {
                        sb.Append(bt.ToString("x2"));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
