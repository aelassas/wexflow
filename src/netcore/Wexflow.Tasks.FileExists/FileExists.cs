using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileExists
{
    public class FileExists : Task
    {
        public string File { get; }

        public FileExists(XElement xe, Workflow wf) : base(xe, wf)
        {
            File = GetSetting("file");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success;

            try
            {
                success = System.IO.File.Exists(File);

                InfoFormat(success ? "The file {0} exists." : "The file {0} does not exist.", File);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking file {0}. Error: {1}", File, e.Message);
                return new TaskStatus(Status.Error, false);
            }
            finally
            {
                WaitOne();
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
