using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesEqual
{
    public class FilesEqual : Task
    {
        public string File1 { get; set; }
        public string File2 { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesEqual(XElement xe, Workflow wf) : base(xe, wf)
        {
            File1 = GetSetting("file1");
            File2 = GetSetting("file2");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking...");

            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CheckFiles();
                    }
                }
                else
                {
                    success = CheckFiles();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking files. Error: {0}", e.Message);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool CheckFiles()
        {
            if (!File.Exists(File1))
            {
                Logger.ErrorFormat("The file {0} does not exist.", File1);
                return false;
            }

            if (!File.Exists(File2))
            {
                Logger.ErrorFormat("The file {0} does not exist.", File2);
                return false;
            }

            var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"FilesEqual_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
            var xdoc = new XDocument(new XElement("Root"));
            var xFiles = new XElement("Files");

            xFiles.Add(new XElement("File",
                new XAttribute("path", File1),
                new XAttribute("name", Path.GetFileName(File1))));

            xFiles.Add(new XElement("File",
                new XAttribute("path", File2),
                new XAttribute("name", Path.GetFileName(File2))));

            if (xdoc.Root != null)
            {
                xdoc.Root.Add(xFiles);

                var res = FileEquals(File1, File2);
                xdoc.Root.Add(new XElement("Result", res.ToString().ToLower()));
            }

            xdoc.Save(xmlPath);
            Files.Add(new FileInf(xmlPath, Id));
            InfoFormat("The result has been written in: {0}", xmlPath);

            return true;
        }

        private bool FileEquals(string path1, string path2)
        {
            var file1 = File.ReadAllBytes(path1);
            var file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (var i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
