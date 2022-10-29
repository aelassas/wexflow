using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FolderExists
{
    public class FolderExists : Task
    {
        public string Folder { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

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

            var success = false;
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

            if (status != null)
            {
                return status;
            }
            return new TaskStatus(Status.Success, success);
        }

        private bool CheckFolder(ref TaskStatus status)
        {
            var success = false;
            try
            {
                success = System.IO.Directory.Exists(Folder);

                if (success)
                {
                    InfoFormat("The folder {0} exists.", Folder);
                }
                else
                {
                    InfoFormat("The folder {0} does not exist.", Folder);
                }

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
