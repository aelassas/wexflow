using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.FolderExists
{
    public class FolderExists : Task
    {
        public string Folder { get; private set; }

        public FolderExists(XElement xe, Workflow wf): base(xe, wf)
        {
            Folder = GetSetting("folder");
        }

        public override TaskStatus Run()
        {
            Info("Checking folder...");
            
            bool success;

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
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
