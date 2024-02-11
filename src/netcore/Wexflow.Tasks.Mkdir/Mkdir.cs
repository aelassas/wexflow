using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Mkdir
{
    public class Mkdir : Task
    {
        public string[] Folders { get; }

        public Mkdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Info("Creating folders...");

            var success = true;
            var atLeastOneSucceed = false;

            foreach (var folder in Folders)
            {
                try
                {
                    if (!Directory.Exists(folder))
                    {
                        _ = Directory.CreateDirectory(folder ?? throw new InvalidOperationException());
                    }

                    InfoFormat("Folder {0} created.", folder);

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the folder {0}", e, folder);
                    success = false;
                }
                finally
                {
                    WaitOne();
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
