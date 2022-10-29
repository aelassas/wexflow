using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Mkdir
{
    public class Mkdir:Task
    {
        public string[] Folders { get; private set; }

        public Mkdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Info("Creating folders...");
            
            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (string folder in Folders)
            {
                try
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    InfoFormat("Folder {0} created.", folder);
                    
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the folder {0}", e, folder);
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
    }
}
