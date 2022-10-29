using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;

namespace Wexflow.Tasks.FilesExist
{
    public class FilesExist:Task
    {
        public string[] FFiles { get; private set; }
        public string[] Folders { get; private set; }

        public FilesExist(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            FFiles = GetSettings("file");
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Info("Checking...");

            bool success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                       string.Format("FilesExist_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));
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
    }
}
