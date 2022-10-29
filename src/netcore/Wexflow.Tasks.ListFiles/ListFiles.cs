using System;
using System.Collections.Generic;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.ListFiles
{
    public class ListFiles:Task
    {
        public ListFiles(XElement xe, Workflow wf)
            : base(xe, wf)
        {
		}

        public override TaskStatus Run()
        {
            Info("Listing files...");
            //System.Threading.Thread.Sleep(10 * 1000);

            bool success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("ListFiles_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("WexflowProcessing"));

                var xWorkflow = new XElement("Workflow",
                    new XAttribute("id", Workflow.Id),
                    new XAttribute("name", Workflow.Name),
                    new XAttribute("description", Workflow.Description));

                var xFiles = new XElement("Files");
                foreach (List<FileInf> files in Workflow.FilesPerTask.Values)
                {
                    foreach (FileInf file in files)
                    {
                        xFiles.Add(file.ToXElement());
                        Info(file.ToString());
                    }
                }

                xWorkflow.Add(xFiles);
                if(xdoc.Root != null) xdoc.Root.Add(xWorkflow);
                xdoc.Save(xmlPath);

                var xmlFile = new FileInf(xmlPath, Id);
                Files.Add(xmlFile);
                Info(xmlFile.ToString());
            }
            catch (ThreadAbortException)
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
