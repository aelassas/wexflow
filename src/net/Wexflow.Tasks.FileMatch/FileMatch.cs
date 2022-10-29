using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileMatch
{
    public class FileMatch : Task
    {
        public string Dir { get; private set; }
        public string Pattern { get; private set; }
        public bool Recursive { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public FileMatch(XElement xe, Workflow wf) : base(xe, wf)
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

            var success = false;
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
            var success = false;
            string fileFound = string.Empty;

            try
            {
                string[] files;

                if (Recursive)
                {
                    files = Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    files = Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly);
                }

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
