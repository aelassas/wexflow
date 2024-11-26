using System.Diagnostics;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ExecPython
{
    public class ExecPython : Core.Task
    {
        public string PythonPath { get; }

        public ExecPython(XElement xe, Workflow wf) : base(xe, wf)
        {
            PythonPath = GetSetting("pythonPath");
        }

        public override Core.TaskStatus Run()
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
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while executing the script {0}: {1}", pythonFile.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
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

        private bool Exec(string pyScriptPath)
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
