using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.Management;
using System.IO;

namespace Wexflow.Tasks.Wmi
{
    public class Wmi:Task
    {
        public string Query { get; private set; }

        public Wmi(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Query = GetSetting("query");
        }

        public override TaskStatus Run()
        {
            Info("Running WMI query...");

            bool success = true;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder, string.Format("WMI_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Objects"));
                var searcher = new ManagementObjectSearcher(Query);
                var collection = searcher.Get();

                foreach (var o in collection)
                {
                    var obj = (ManagementObject) o;
                    var xObj = new XElement("Object");
                    foreach (PropertyData prop in obj.Properties)
                    {
                        var xProp = new XElement("Property", new XAttribute("name", prop.Name), new XAttribute("value", prop.Value ?? string.Empty));
                        xObj.Add(xProp);
                    }
                    if(xdoc.Root==null)throw new Exception("Root node does not exist.");
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
