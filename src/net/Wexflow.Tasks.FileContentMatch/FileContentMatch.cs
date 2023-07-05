using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileContentMatch
{
    public class FileContentMatch : Task
    {
        public string[] FilesToCheck { get; }
        public string[] FoldersToCheck { get; }
        public bool Recursive { get; }
        public string Pattern { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FileContentMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            FilesToCheck = GetSettings("file");
            FoldersToCheck = GetSettings("folder");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
            Pattern = GetSetting("pattern");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success;
            TaskStatus status = null;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CheckFile(ref status);
                    }
                }
                else
                {
                    success = CheckFile(ref status);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the file.", e);
                success = false;
            }

            Info("Task finished");

            return status ?? new TaskStatus(Status.Success, success);
        }

        private bool CheckFile(ref TaskStatus status)
        {
            var success = true;
            try
            {
                // Checking files
                foreach (var file in FilesToCheck)
                {
                    var res = Regex.Match(File.ReadAllText(file), Pattern, RegexOptions.Multiline).Success;

                    if (res)
                    {
                        InfoFormat("A content matching the pattern {0} was found in the file {1}.", Pattern, file);
                        Files.Add(new FileInf(file, Id));
                    }
                    else
                    {
                        InfoFormat("No content matching the pattern {0} was found in the file {1}.", Pattern, file);
                    }
                }

                // Checking folders
                foreach (var folder in FoldersToCheck)
                {
                    var files = Recursive ? Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories) : Directory.GetFiles(folder);

                    foreach (var file in files)
                    {
                        var res = Regex.Match(File.ReadAllText(file), Pattern, RegexOptions.Multiline).Success;

                        if (res)
                        {
                            InfoFormat("A content matching the pattern {0} was found in the file {1}.", Pattern, file);
                            Files.Add(new FileInf(file, Id));
                        }
                        else
                        {
                            InfoFormat("No content matching the pattern {0} was found in the file {1}.", Pattern, file);
                        }
                    }
                }

                if (Files.Count == 0)
                {
                    success = false;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking thes files. Error: {0}", e.Message);
                status = new TaskStatus(Status.Error, false);
            }

            return success;
        }
    }
}
