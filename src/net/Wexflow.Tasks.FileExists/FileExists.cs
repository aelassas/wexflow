using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.FileExists
{
    public class FileExists : Task
    {
        public string File { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

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

            var success = false;

            try
            {

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
                    success = false;
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking file {0}. Error: {1}", File, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }

        private bool CheckFile()
        {
            var success = System.IO.File.Exists(File);

            if (success)
            {
                InfoFormat("The file {0} exist.", File);
            }
            else
            {
                InfoFormat("The file {0} does not exist.", File);
            }

            return success;
        }
    }
}
