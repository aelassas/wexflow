using Quartz;

namespace Wexflow.Core
{
    /// <summary>
    /// Quartz Workflow Job.
    /// </summary>
    public class WorkflowJob : IJob
    {
        /// <summary>
        /// Executes workflow the job
        /// </summary>
        /// <param name="context">Job context.</param>
        System.Threading.Tasks.Task IJob.Execute(IJobExecutionContext context)
        {
            var workflow = (Workflow)context.JobDetail.JobDataMap.Get("workflow");

            System.Threading.Tasks.Task task = new(() =>
            {
                _ = workflow.StartAsync(workflow.WexflowEngine.SuperAdminUsername);
            });
            task.Start();

            return task;
        }
    }
}
