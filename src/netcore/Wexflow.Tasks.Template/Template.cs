using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Template
{
    public class Template : Task
    {
        public Template(XElement xe, Workflow wf) : base(xe, wf)
        {
            // Initialize task settings from the XML element if needed
            // Example: string settingValue = GetSetting("mySetting");
        }

        public override TaskStatus Run()
        {
            try
            {
                // Task logic goes here
                Info("Running my custom task...");

                // WaitOne() enables suspend/resume support in .NET 8.0+.
                // Call this to pause the task when the workflow is suspended.
                WaitOne();

                // Return success when the task completes successfully
                return new TaskStatus(Status.Success);
            }
            catch (ThreadInterruptedException)
            {
                // Required for proper stop handling (do not swallow this exception)
                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected errors and return error status
                ErrorFormat("An error occurred while executing the task.", ex);
                return new TaskStatus(Status.Error);
            }
        }
    }
}
