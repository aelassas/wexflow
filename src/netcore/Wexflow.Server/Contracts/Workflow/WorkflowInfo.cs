namespace Wexflow.Server.Contracts.Workflow
{
    public class WorkflowInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int LaunchType { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsApproval { get; set; }
        public bool EnableParallelJobs { get; set; }
        public string Description { get; set; }
        public string Period { get; set; }
        public string CronExpression { get; set; }
        public int RetryCount { get; set; }
        public int RetryTimeout { get; set; }
        public Variable[] LocalVariables { get; set; }
    }
}