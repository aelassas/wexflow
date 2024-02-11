using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ListFiles
{
    public class ListFiles : Task
    {
        public ListFiles(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Listing files...");
            //System.Threading.Thread.Sleep(10 * 1000);

            var success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"ListFiles_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                XDocument xdoc = new(new XElement("WexflowProcessing"));

                XElement xWorkflow = new("Workflow",
                    new XAttribute("id", Workflow.Id),
                    new XAttribute("name", Workflow.Name),
                    new XAttribute("description", Workflow.Description));

                XElement xFiles = new("Files");
                foreach (var files in Workflow.FilesPerTask.Values)
                {
                    foreach (var file in files)
                    {
                        xFiles.Add(file.ToXElement());
                        Info(file.ToString());
                        WaitOne();
                    }
                }

                xWorkflow.Add(xFiles);
                xdoc.Root?.Add(xWorkflow);
                xdoc.Save(xmlPath);

                FileInf xmlFile = new(xmlPath, Id);
                Files.Add(xmlFile);
                Info(xmlFile.ToString());
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while listing files. Error: {0}", e.Message);
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
