using System.Diagnostics;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ExecPython
{
    public class ExecPython : Core.Task
    {
        public string PythonPath { get; private set; }

        public ExecPython(XElement xe, Workflow wf) : base(xe, wf)
        {
            PythonPath = GetSetting("pythonPath");
        }

        public override Core.TaskStatus Run()
        {
            Info("Executing python scripts...");

            Status status = Status.Success;
            bool success = true;
            bool atLeastOneSuccess = false;
            FileInf[] pythonFiles = SelectFiles();

            foreach (FileInf? pythonFile in pythonFiles)
            {
                try
                {
                    Exec(pythonFile.Path);
                    InfoFormat("The script {0} has been executed.", pythonFile.Path);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
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
            return new Core.TaskStatus(status);
        }

        private void Exec(string pyScriptPath)
        {
            ProcessStartInfo startInfo = new(PythonPath, pyScriptPath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
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
