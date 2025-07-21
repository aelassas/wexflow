using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Rmdir
{
    public class Rmdir : Task
    {
        public string[] Folders { get; }

        public Rmdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Folders = GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Info("Removing folders...");

            var success = true;
            var atLeastOneSucceed = false;

            foreach (var folder in Folders)
            {
                try
                {
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    RmdirRec(folder);
                    InfoFormat("Folder {0} deleted.", folder);

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while deleting the folder {0}", e, folder);
                    success = false;
                }
                finally
                {
                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
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

        private static void RmdirRec(string folder)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                RmdirRec(dir);
            }

            Directory.Delete(folder);
        }
    }
}
