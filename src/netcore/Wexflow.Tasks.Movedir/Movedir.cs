using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Movedir
{
    public class Movedir : Task
    {
        public string Folder { get; }
        public string DestinationFolder { get; }
        public bool Overwrite { get; }

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

            var succeeded = false;
            try
            {
                var move = true;
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
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while moving the folder {0} to {1}. Error: {2}", Folder, DestinationFolder, e.Message);
            }
            finally
            {
                WaitOne();
            }

            Info("Task finished.");
            return new TaskStatus(succeeded ? Status.Success : Status.Error, false);
        }

        private static void DeleteRec(string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }

            foreach (var subdir in Directory.GetDirectories(dir))
            {
                DeleteRec(subdir);
            }

            Directory.Delete(dir);
        }
    }
}
