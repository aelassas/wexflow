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

        public FilesExist(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            FFiles = GetSettings("file");
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Info("Checking...");

            var success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"FilesExist_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                XDocument xdoc = new(new XElement("Root"));
                XElement xFiles = new("Files");
                XElement xFolders = new("Folders");

                foreach (var file in FFiles)
                {
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    xFiles.Add(new XElement("File",
                        new XAttribute("path", file),
                        new XAttribute("name", Path.GetFileName(file)),
                        new XAttribute("exists", File.Exists(file))));
                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
                }

                foreach (var folder in Folders)
                {
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    xFolders.Add(new XElement("Folder",
                        new XAttribute("path", folder),
                        new XAttribute("name", Path.GetFileName(folder)),
                        new XAttribute("exists", Directory.Exists(folder))));
                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
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
            catch (OperationCanceledException)
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
