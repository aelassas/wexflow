using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesExist
{
    public class FilesExist : Task
    {
        public string[] FFiles { get; }
        public string[] Folders { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesExist(XElement xe, Workflow wf) : base(xe, wf)
        {
            FFiles = GetSettings("file");
            Folders = GetSettings("folder");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking...");

            var success = true;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        CheckFiles();
                    }
                }
                else
                {
                    CheckFiles();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking files and folders. Error: {0}", e.Message);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private void CheckFiles()
        {
            var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"FilesExist_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
            var xdoc = new XDocument(new XElement("Root"));
            var xFiles = new XElement("Files");
            var xFolders = new XElement("Folders");

            foreach (var file in FFiles)
            {
                xFiles.Add(new XElement("File",
                    new XAttribute("path", file),
                    new XAttribute("name", Path.GetFileName(file)),
                    new XAttribute("exists", File.Exists(file))));
            }

            foreach (var folder in Folders)
            {
                xFolders.Add(new XElement("Folder",
                    new XAttribute("path", folder),
                    new XAttribute("name", Path.GetFileName(folder)),
                    new XAttribute("exists", Directory.Exists(folder))));
            }

            if (xdoc.Root != null)
            {
                xdoc.Root.Add(xFiles);
                xdoc.Root.Add(xFolders);
            }

            xdoc.Save(xmlPath);
            Files.Add(new FileInf(xmlPath, Id));
            InfoFormat("The result has been written in: {0}", xmlPath);
        }
    }
}
