using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.CsvToXml
{
    public class CsvToXml : Task
    {
        public CsvToXml(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Creating XML files...");

            var success = true;
            var atLeastOneSucceed = false;

            foreach (var file in SelectFiles())
            {
                try
                {
                    var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                    CreateXml(file.Path, xmlPath);
                    Files.Add(new FileInf(xmlPath, Id));
                    InfoFormat("XML file {0} created from {1}", xmlPath, file.Path);
                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the XML from {0} Please check this XML file according to the documentation of the task. Error: {1}", file.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
                }
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

        private static void CreateXml(string csvPath, string xmlPath)
        {
            XDocument xdoc = new(new XElement("Lines"));

            foreach (var line in File.ReadAllLines(csvPath))
            {
                XElement xLine = new("Line");
                foreach (var col in line.Split(';'))
                {
                    if (!string.IsNullOrEmpty(col))
                    {
                        xLine.Add(new XElement("Column", col));
                    }
                }
                if (xdoc.Root == null)
                {
                    throw new Exception("No root node found.");
                }

                xdoc.Root.Add(xLine);
            }

            xdoc.Save(xmlPath);
        }
    }
}
