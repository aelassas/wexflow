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
                string xmlPath = Path.Combine(Workflow.WorkflowTempFolder, string.Format("WMI_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                XDocument xdoc = new XDocument(new XElement("Objects"));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementBaseObject o in collection)
                {
                    ManagementObject obj = (ManagementObject)o;
                    XElement xObj = new XElement("Object");
                    foreach (PropertyData prop in obj.Properties)
                    {
                        XElement xProp = new XElement("Property", new XAttribute("name", prop.Name), new XAttribute("value", prop.Value ?? string.Empty));
                        xObj.Add(xProp);
                    }
                    if (xdoc.Root == null) throw new Exception("Root node does not exist.");
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

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
