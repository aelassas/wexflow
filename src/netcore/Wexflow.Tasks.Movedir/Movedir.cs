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

        public Movedir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Folder = GetSetting("folder");
            DestinationFolder = GetSetting("destinationFolder");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Moving directory...");

            bool succeeded = false;
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
                    succeeded = true;
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

            Info("Task finished.");
            return new TaskStatus(succeeded ? Status.Success : Status.Error, false);
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
