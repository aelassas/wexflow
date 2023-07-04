using System;
using System.IO;
using System.Management;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Wmi
{
    public class Wmi : Task
    {
        public string Query { get; }

        public Wmi(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Query = GetSetting("query");
        }

        public override TaskStatus Run()
        {
            Info("Running WMI query...");

            var success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"WMI_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                var xdoc = new XDocument(new XElement("Objects"));
                var searcher = new ManagementObjectSearcher(Query);
                var collection = searcher.Get();

                foreach (var o in collection)
                {
                    var obj = (ManagementObject)o;
                    var xObj = new XElement("Object");
                    foreach (var prop in obj.Properties)
                    {
                        var xProp = new XElement("Property", new XAttribute("name", prop.Name), new XAttribute("value", prop.Value ?? string.Empty));
                        xObj.Add(xProp);
                    }
                    if (xdoc.Root == null)
                    {
                        throw new Exception("Root node does not exist.");
                    }

                    xdoc.Root.Add(xObj);
                }
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("The query {0} has been executed.", Query);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the query {0}. Error: {1}", Query, e.Message);
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
