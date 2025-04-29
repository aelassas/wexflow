using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileNotExist
{
    public class FileNotExist : Core.Task
    {
        public string File { get; }

        public FileNotExist(XElement xe, Workflow wf) : base(xe, wf)
        {
            File = GetSetting("file");
        }

        public override Core.TaskStatus Run()
        {
            Info("Checking file...");

            bool fileExists;

            try
            {
                fileExists = System.IO.File.Exists(File);

                InfoFormat(fileExists ? "The file {0} exists." : "The file {0} does not exist.", File);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking file {0}. Error: {1}", File, e.Message);
                return new Core.TaskStatus(Status.Error, false);
            }
            finally
            {
                WaitOne();
            }

            Info("Task finished");

            return new Core.TaskStatus(Status.Success, !fileExists);
        }
    }
}
