using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;

namespace Wexflow.Tasks.Guid
{
    public class Guid : Task
    {
        public int GuidCount { get; private set; }

        public Guid(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            GuidCount = int.Parse(GetSetting("guidCount", "1"));
        }

        public override TaskStatus Run()
        {
            Info("Generating Guids...");
            
            var guidPath = Path.Combine(Workflow.WorkflowTempFolder,
                string.Format("Guid_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

            var xguids = new XElement("Guids");

            for (int i = 0; i < GuidCount; i++)
            {
                xguids.Add(new XElement("Guid", System.Guid.NewGuid()));
            }

            var xdoc = new XDocument(xguids);
            xdoc.Save(guidPath);
            Files.Add(new FileInf(guidPath, Id));

            Info("Task finished.");
            return new TaskStatus(Status.Success, false);
        }
    }
}
