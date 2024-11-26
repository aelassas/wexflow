using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ProcessLauncher
{
    public partial class ProcessLauncher : Task
    {
        public string ProcessPath { get; set; }
        public string ProcessCmd { get; set; }
        public bool HideGui { get; set; }
        public bool GeneratesFiles { get; set; }
        public bool LoadAllFiles { get; set; }
        public bool IgnoreExitCode { get; set; }

        private const string VAR_FILE_PATH = "$filePath";
        private const string VAR_FILE_NAME = "$fileName";
        private const string VAR_FILE_NAME_WITHOUT_EXTENSION = "$fileNameWithoutExtension";
        private const string VAR_OUTPUT = "$output";

        public ProcessLauncher(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ProcessPath = GetSetting("processPath");
            ProcessCmd = GetSetting("processCmd");
            HideGui = bool.Parse(GetSetting("hideGui"));
            GeneratesFiles = bool.Parse(GetSetting("generatesFiles"));
            LoadAllFiles = bool.Parse(GetSetting("loadAllFiles", "false"));
            IgnoreExitCode = bool.Parse(GetSetting("ignoreExitCode", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Launching process...");

            if (GeneratesFiles && !(ProcessCmd.Contains(VAR_FILE_NAME) && ProcessCmd.Contains(VAR_OUTPUT) && (ProcessCmd.Contains(VAR_FILE_NAME) || ProcessCmd.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION))))
            {
                Error("Error in process command. Please read the documentation.");
                return new TaskStatus(Status.Error, false);
            }

            var success = true;
            var atLeastOneSucceed = false;

            if (!GeneratesFiles)
            {
                return StartProcess(ProcessPath, ProcessCmd, HideGui);
            }

            foreach (var file in SelectFiles())
            {
                string cmd;
                string outputFilePath;

                try
                {
                    cmd = ProcessCmd.Replace($"{{{VAR_FILE_PATH}}}", $"\"{file.Path}\"");

                    var outputRegex = OutputRegex();
                    var m = outputRegex.Match(cmd);

                    if (m.Success)
                    {
                        var val = m.Value;
                        outputFilePath = val;
                        if (outputFilePath.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION))
                        {
                            outputFilePath = outputFilePath.Replace(VAR_FILE_NAME_WITHOUT_EXTENSION, Path.GetFileNameWithoutExtension(file.FileName));
                        }
                        else if (outputFilePath.Contains(VAR_FILE_NAME))
                        {
                            outputFilePath = outputFilePath.Replace(VAR_FILE_NAME, file.FileName);
                        }
                        outputFilePath = outputFilePath.Replace($"{{{VAR_OUTPUT}:", Workflow.WorkflowTempFolder.Trim('\\') + "\\");
                        outputFilePath = outputFilePath.Trim('}');

                        cmd = cmd.Replace(val, $"\"{outputFilePath}\"");
                    }
                    else
                    {
                        Error("Error in process command. Please read the documentation.");
                        return new TaskStatus(Status.Error, false);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("Error in process command. Please read the documentation. Error: {0}", e.Message);
                    return new TaskStatus(Status.Error, false);
                }
                finally
                {
                    WaitOne();
                }

                if (StartProcess(ProcessPath, cmd, HideGui).Status == Status.Success)
                {
                    Files.Add(new FileInf(outputFilePath, Id));

                    if (LoadAllFiles)
                    {
                        var files = Directory.GetFiles(Workflow.WorkflowTempFolder, "*.*", SearchOption.AllDirectories);

                        foreach (var f in files)
                        {
                            if (f != outputFilePath)
                            {
                                Files.Add(new FileInf(f, Id));
                            }
                        }
                    }

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                else
                {
                    success = false;
                }
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private TaskStatus StartProcess(string processPath, string processCmd, bool hideGui)
        {
            try
            {
                ProcessStartInfo startInfo = new(processPath, processCmd)
                {
                    CreateNoWindow = hideGui,
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
                InfoFormat("ExitCode: {0}", process.ExitCode);

                var status = process.ExitCode == 0 || IgnoreExitCode ? Status.Success : Status.Error;
                return new TaskStatus(status, false);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while launching the process {0}", e, processPath);
                return new TaskStatus(Status.Error, false);
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

        [GeneratedRegex("{\\$output:(?:\\$fileNameWithoutExtension|\\$fileName)(?:[a-zA-Z0-9._-]*})")]
        private static partial Regex OutputRegex();
    }
}
