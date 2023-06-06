using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Sha1
{
    public class Sha1 : Task
    {
        public Sha1(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating SHA-1 hashes...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("SHA1_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                XDocument xdoc = new(new XElement("Files"));
                foreach (var file in files)
                {
                    try
                    {
                        var sha1 = GetSha1(file.Path);
                        xdoc.Root?.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("sha1", sha1)));
                        InfoFormat("SHA-1 hash of the file {0} is {1}", file.Path, sha1);

                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the SHA-1 hash of the file {0}", e, file.Path);
                        success = false;
                    }
                }
                xdoc.Save(md5Path);
                Files.Add(new FileInf(md5Path, Id));
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

        private static string GetSha1(string filePath)
        {
            StringBuilder sb = new();
            using (SHA1Managed sha1 = new())
            {
                using var stream = File.OpenRead(filePath);
                var bytes = sha1.ComputeHash(stream);

                foreach (var bt in bytes)
                {
                    _ = sb.Append(bt.ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}
