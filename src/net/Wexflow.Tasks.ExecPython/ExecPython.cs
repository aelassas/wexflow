using System;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ExecPython
{
    public class ExecPython : Task
    {
        public string PythonPath { get; }

        public ExecPython(XElement xe, Workflow wf) : base(xe, wf)
        {
            PythonPath = GetSetting("pythonPath");
        }

        public override TaskStatus Run()
        {
            Info("Executing python scripts...");

            var status = Status.Success;
            var success = true;
            var atLeastOneSuccess = false;
            var pythonFiles = SelectFiles();

            foreach (var pythonFile in pythonFiles)
            {
                try
                {
                    var res = Exec(pythonFile.Path);
                    InfoFormat("The script {0} has been executed.", pythonFile.Path);
                    if (res && !atLeastOneSuccess)
                    {
                        atLeastOneSuccess = true;
                    }
                    success &= res;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while executing the script {0}: {1}", pythonFile.Path, e.Message);
                    success = false;
                }
            }

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool Exec(string pyScriptPath)
        {
            var startInfo = new ProcessStartInfo(PythonPath, pyScriptPath)
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
            InfoFormat("ExitCode for {0}: {1}", pyScriptPath, process.ExitCode);

            var res = process.ExitCode == 0;
            return res;
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
