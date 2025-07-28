using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;

namespace Wexflow.Tasks.Template
{
    public class Template : Task
    {
        public Template(XElement xe, Workflow wf): base(xe, wf)
        {
            // Initialize task settings from the XML element if needed.
            // Example: string settingValue = GetSetting("mySetting");
        }

        public async override System.Threading.Tasks.Task<TaskStatus> RunAsync()
        {
            try
            {
                // Main task logic goes here.
                Info("Running my custom task...");

                // Simulate work using asynchronous delay.
                await System.Threading.Tasks.Task.Delay(2000);

                return new TaskStatus(Status.Success);
            }
            catch (ThreadInterruptedException)
            {
                // Don't suppress this exception; it allows proper workflow stop handling.
                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected errors and return error status.
                ErrorFormat("An error occured.", ex);
                return new TaskStatus(Status.Error);
            }
        }
    }
}
