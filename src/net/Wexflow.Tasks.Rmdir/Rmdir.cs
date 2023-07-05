using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Rmdir
{
    public class Rmdir : Task
    {
        public string[] Folders { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public Rmdir(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folders = GetSettings("folder");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Removing folders...");

            bool success;
            var atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = RemoveFolders(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = RemoveFolders(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while removing folders.", e);
                success = false;
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

        private bool RemoveFolders(ref bool atLeastOneSucceed)
        {
            var success = true;
            foreach (var folder in Folders)
            {
                try
                {
                    RmdirRec(folder);
                    InfoFormat("Folder {0} deleted.", folder);

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while deleting the folder {0}", e, folder);
                    success = false;
                }
            }
            return success;
        }

        private void RmdirRec(string folder)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                RmdirRec(dir);
            }

            Directory.Delete(folder);
        }
    }
}
