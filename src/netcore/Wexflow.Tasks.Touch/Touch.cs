using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Touch
{
    public class Touch:Task
    {
        public string[] Tfiles { get; private set; }

        public Touch(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Tfiles = GetSettings("file");
        }

        public override TaskStatus Run()
        {
            Info("Touching files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (string file in Tfiles)
            {
                try
                {
                    TouchFile(file);
                    InfoFormat("File {0} created.", file);
                    Files.Add(new FileInf(file, Id));
                    
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the file {0}", e, file);
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

        private void TouchFile(string file)
        {
            using (File.Create(file)) { }
        }
    }
}
