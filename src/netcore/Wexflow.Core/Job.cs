using System;

namespace Wexflow.Core
{
    /// <summary>
    /// Workflow job used for queuing.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// Workflow.
        /// </summary>
        public Workflow Workflow { get; set; }
        /// <summary>
        /// Queued on date time.
        /// </summary>
        public DateTime QueuedOn { get; set; }
    }
}
