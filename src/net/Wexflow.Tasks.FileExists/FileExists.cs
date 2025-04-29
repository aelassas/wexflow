using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileExists
{
    public class FileExists : Task
    {
        public string File { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FileExists(XElement xe, Workflow wf) : base(xe, wf)
        {
            File = GetSetting("file");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success = false;
            TaskStatus status = null;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CheckFile();
                    }
                }
                else
                {
                    success = CheckFile();
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

            return status ?? new TaskStatus(Status.Success, success);
        }

        private bool CheckFile()
        {
            var success = System.IO.File.Exists(File);

            InfoFormat(success ? "The file {0} exists." : "The file {0} does not exist.", File);

            return success;
        }
    }
}
