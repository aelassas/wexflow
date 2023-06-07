using System;
using System.Diagnostics;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ProcessKiller
{
    public class ProcessKiller : Task
    {
        public static string ProcessName { get; private set; }

        public ProcessKiller(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ProcessName = GetSetting("processName");
        }

        public override TaskStatus Run()
        {
            try
            {
                var processCmd = $"/im \"{ProcessName}\" /f";
                var startInfo = new ProcessStartInfo("taskkill", processCmd)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new Process { StartInfo = startInfo };
                process.OutputDataReceived += OutputHandler;
                process.ErrorDataReceived += ErrorHandler;
                _ = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                Logger.InfoFormat("The process {0} was killed.", ProcessName);

                return new TaskStatus(Status.Success);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while killing the process {0}.", e, ProcessName);
                return new TaskStatus(Status.Error);
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            InfoFormat("{0}", outLine.Data);
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ErrorFormat("{0}", outLine.Data);
        }
    }
}