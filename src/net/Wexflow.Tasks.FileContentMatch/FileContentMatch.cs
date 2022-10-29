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
        public string[] FilesToCheck { get; private set; }
        public string[] FoldersToCheck { get; private set; }
        public bool Recursive { get; private set; }
        public string Pattern { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

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

            var success = true;
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

            if (status != null)
            {
                return status;
            }
            return new TaskStatus(Status.Success, success);
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
                    var files = new string[] { };
                    if (Recursive)
                    {
                        files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories); ;
                    }
                    else
                    {
                        files = Directory.GetFiles(folder);
                    }

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
