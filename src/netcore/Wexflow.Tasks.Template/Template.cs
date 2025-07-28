using System;
using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;

namespace Wexflow.Tasks.Template
{
    public class Template : Task
    {
        public Template(XElement xe, Workflow wf) : base(xe, wf)
        {
            // Initialize task settings from the XML element if needed.
            // Example: string settingValue = GetSetting("mySetting");
        }

        public async override System.Threading.Tasks.Task<TaskStatus> RunAsync()
        {
            try
            {
                // Check for workflow cancellation at the start of execution.
                // Always include this check in any long-running or looped logic.
                Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                // Main task logic goes here.
                Info("Running my custom task...");

                // Simulate work using asynchronous delay.
                await System.Threading.Tasks.Task.Delay(2000);

                // Support workflow suspension. This call will block if the workflow is paused.
                // Only call WaitOne if cancellation hasn't already been requested.
                if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    WaitOne();
                }

                // Return success when the task completes successfully
                return new TaskStatus(Status.Success);
            }
            catch (OperationCanceledException)
            {
                // Don't suppress this exception; it allows proper workflow stop handling.
                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected errors and return error status.
                ErrorFormat("An error occurred while executing the task.", ex);
                return new TaskStatus(Status.Error);
            }
        }
    }
}
