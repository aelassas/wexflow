using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FolderExists
{
    public class FolderExists : Task
    {
        public string Folder { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FolderExists(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folder = GetSetting("folder");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking folder...");

            bool success;
            TaskStatus status = null;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CheckFolder(ref status);
                    }
                }
                else
                {
                    success = CheckFolder(ref status);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the folder.", e);
                success = false;
            }

            Info("Task finished");

            return status ?? new TaskStatus(Status.Success, success);
        }

        private bool CheckFolder(ref TaskStatus status)
        {
            var success = false;
            try
            {
                success = System.IO.Directory.Exists(Folder);

                InfoFormat(success ? "The folder {0} exists." : "The folder {0} does not exist.", Folder);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the folder {0}. Error: {1}", Folder, e.Message);
                status = new TaskStatus(Status.Error, false);
            }

            return success;
        }
    }
}
