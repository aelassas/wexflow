using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Rmdir
{
    public class Rmdir:Task
    {
        public string[] Folders { get; private set; }

        public Rmdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Info("Removing folders...");
            
            bool success = true;
            bool atLeastOneSucceed = false;

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

        private void RmdirRec(string folder)
        {
            foreach (string file in Directory.GetFiles(folder)) File.Delete(file);
            foreach (string dir in Directory.GetDirectories(folder)) RmdirRec(dir);
            Directory.Delete(folder);
        }
    }
}
