using System;
using System.IO;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Guid
{
    public class Guid : Task
    {
        public int GuidCount { get; }

        public Guid(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            GuidCount = int.Parse(GetSetting("guidCount", "1"));
        }

        public override TaskStatus Run()
        {
            Info("Generating Guids...");

            var guidPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"Guid_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

            XElement xguids = new("Guids");

            for (var i = 0; i < GuidCount; i++)
            {
                xguids.Add(new XElement("Guid", System.Guid.NewGuid()));
                WaitOne();
            }

            XDocument xdoc = new(xguids);
            xdoc.Save(guidPath);
            Files.Add(new FileInf(guidPath, Id));

            Info("Task finished.");
            return new TaskStatus(Status.Success, false);
        }
    }
}
