using System;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Wexflow.Tasks.Md5
{
    public class Md5:Task
    {
        public Md5(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating MD5 sums...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("MD5_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        var md5 = GetMd5(file.Path);
                        if (xdoc.Root != null)
                        {
                            xdoc.Root.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("md5", md5)));
                        }
                        InfoFormat("Md5 of the file {0} is {1}", file.Path, md5);
                        
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the md5 of the file {0}", e, file.Path);
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

        private string GetMd5(string filePath)
        {
            var sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    var bytes = md5.ComputeHash(stream);

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
