using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Rmdir
{
    public class Rmdir : Task
    {
        public string[] Folders { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

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

            var success = true;
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
            foreach (string folder in Folders)
            {
                try
                {
                    RmdirRec(folder);
                    InfoFormat("Folder {0} deleted.", folder);

                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
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
            foreach (string file in Directory.GetFiles(folder)) File.Delete(file);
            foreach (string dir in Directory.GetDirectories(folder)) RmdirRec(dir);
            Directory.Delete(folder);
        }
    }
}
