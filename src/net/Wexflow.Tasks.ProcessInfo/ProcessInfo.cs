using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ProcessInfo
{
    public class ProcessInfo : Task
    {
        public static string ProcessName { get; private set; }

        public ProcessInfo(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ProcessName = GetSetting("processName");
        }

        public override TaskStatus Run()
        {
            try
            {
                Info("Generating process information...");

                var processes = Process.GetProcessesByName(ProcessName);

                var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"ProcessInfo_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                var xprocesses = new XElement("Processes");
                foreach (var process in processes)
                {
                    var xprocess = new XElement("Process"
                        , new XAttribute("id", process.Id)
                        , new XAttribute("processName", process.ProcessName)
                        , new XAttribute("fileName", process.MainModule != null ? process.MainModule.FileName : string.Empty)
                        , new XAttribute("startTime", $"{process.StartTime:yyyy-MM-dd HH:mm:ss.fff}")
                        , new XAttribute("machineName", process.MachineName)
                        , new XAttribute("sessionId", process.SessionId)
                        , new XAttribute("mainWindowTitle", process.MainWindowTitle)
                        , new XAttribute("pagedMemorySize64", process.PagedMemorySize64)
                        , new XAttribute("peakVirtualMemorySize64", process.PeakVirtualMemorySize64)
                        , new XAttribute("privateMemorySize64", process.PrivateMemorySize64)
                        , new XAttribute("virtualMemorySize64", process.VirtualMemorySize64)
                        , new XAttribute("priorityBoostEnabled", process.PriorityBoostEnabled)
                        , new XAttribute("threadCount", process.Threads.Count)
                        );
                    xprocesses.Add(xprocess);
                }

                var xdoc = new XDocument(xprocesses);
                xdoc.Save(destPath);
                Files.Add(new FileInf(destPath, Id));
                Info("Task finished.");
                return new TaskStatus(Status.Success);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while killing the process {0}.", e, ProcessName);
                return new TaskStatus(Status.Error);
            }
        }
    }
}