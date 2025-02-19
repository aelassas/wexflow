using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileNotMatch
{
    public class FileNotMatch : Task
    {
        public string Dir { get; }
        public string Pattern { get; }
        public bool Recursive { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FileNotMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Dir = GetSetting("dir");
            Pattern = GetSetting("pattern");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool fileMatch = false;
            TaskStatus status = null;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        fileMatch = CheckFile(ref status);
                    }
                }
                else
                {
                    fileMatch = CheckFile(ref status);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the file.", e);
                status = new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return status ?? new TaskStatus(Status.Success, !fileMatch);
        }

        private bool CheckFile(ref TaskStatus status)
        {
            var success = false;
            var fileFound = string.Empty;

            try
            {
                var files = Recursive
                    ? Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories)
                    : Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (Regex.Match(Path.GetFileName(file), Pattern).Success)
                    {
                        fileFound = file;
                        success = true;
                        break;
                    }
                }

                if (success)
                {
                    InfoFormat("A file matching the pattern {0} was found in the directory {1} -> {2}.", Pattern, Dir, fileFound);
                }
                else
                {
                    InfoFormat("No file was found in the directory {0} matching the pattern {1}.", Dir, Pattern);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking directory {0}. Error: {1}", Dir, e.Message);
                status = new TaskStatus(Status.Error, false);
            }
            return success;
        }
    }
}
