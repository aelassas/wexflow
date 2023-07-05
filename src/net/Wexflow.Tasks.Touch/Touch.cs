using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Touch
{
    public class Touch : Task
    {
        public string[] Tfiles { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public Touch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Tfiles = GetSettings("file");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Touching files...");

            bool success;
            var atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = TouchFiles(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = TouchFiles(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while touching files.", e);
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

        private bool TouchFiles(ref bool atLeastOneSucceed)
        {
            var success = true;
            foreach (var file in Tfiles)
            {
                try
                {
                    TouchFile(file);
                    InfoFormat("File {0} created.", file);
                    Files.Add(new FileInf(file, Id));

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
                    ErrorFormat("An error occured while creating the file {0}", e, file);
                    success = false;
                }
            }
            return success;
        }

        private void TouchFile(string file)
        {
            using (File.Create(file)) { }
        }
    }
}
