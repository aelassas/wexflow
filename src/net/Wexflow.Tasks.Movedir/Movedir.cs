using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Movedir
{
    public class Movedir : Task
    {
        public string Folder { get; private set; }
        public string DestinationFolder { get; private set; }
        public bool Overwrite { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Movedir(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folder = GetSetting("folder");
            DestinationFolder = GetSetting("destinationFolder");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Moving directory...");

            var success = false;
            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = MoveDirectory();
                    }
                }
                else
                {
                    success = MoveDirectory();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while moving folder.", e);
                success = false;
            }

            Info("Task finished.");
            return new TaskStatus(success ? Status.Success : Status.Error, false);
        }

        private bool MoveDirectory()
        {
            var success = false;
            try
            {
                bool move = true;
                if (Directory.Exists(DestinationFolder))
                {
                    if (Overwrite)
                    {
                        DeleteRec(DestinationFolder);
                    }
                    else
                    {
                        ErrorFormat("Folder {0} already exists.", DestinationFolder);
                        move = false;
                    }
                }

                if (move)
                {
                    Directory.Move(Folder, DestinationFolder);
                    success = true;
                    InfoFormat("Folder moved: {0} -> {1}", Folder, DestinationFolder);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while moving the folder {0} to {1}. Error: {2}", Folder, DestinationFolder, e.Message);
            }
            return success;
        }

        private void DeleteRec(string dir)
        {
            foreach (string file in Directory.GetFiles(dir))
                File.Delete(file);

            foreach (string subdir in Directory.GetDirectories(dir))
                DeleteRec(subdir);

            Directory.Delete(dir);
        }
    }
}
