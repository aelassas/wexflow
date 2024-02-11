using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesRemover
{
    public class FilesRemover : Task
    {
        public FilesRemover(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Removing files...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();
            for (var i = files.Length - 1; i > -1; i--)
            {
                var file = files[i];

                try
                {
                    File.Delete(file.Path);
                    _ = Workflow.FilesPerTask[file.TaskId].Remove(file);
                    InfoFormat("File removed: {0}", file.Path);
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
                    ErrorFormat("An error occured while deleting the file {0}", e, file.Path);
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
