namespace Wexflow.Core
{
    /// <summary>
    /// Represents the aggregated result of a workflow task or node execution.
    /// Used in place of <c>ref</c> and <c>out</c> parameters to support asynchronous recursion.
    /// </summary>
    public class RunResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether all executed tasks succeeded.
        /// Initialized to <c>true</c>; updated with logical AND of each task's success status.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether at least one task completed with a warning.
        /// Initialized to <c>false</c>; updated with logical OR of each task's warning status.
        /// </summary>
        public bool Warning { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether any task has failed with an error.
        /// Initialized to <c>false</c>; updated with logical OR of each task's error status.
        /// </summary>
        public bool Error { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether at least one task succeeded.
        /// This is tracked independently of the <see cref="Success"/> flag for conditional flows.
        /// </summary>
        public bool AtLeastOneSucceeded { get; set; } = false;
    }
}
