using System;

namespace Wexflow.Server.Contracts
{
    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic,
        Cron
    }

    public class WorkflowInfo : IComparable<WorkflowInfo>
    {
        public string DbId { get; set; }

        public int Id { get; set; }

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public LaunchType LaunchType { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsApproval { get; private set; }

        public bool EnableParallelJobs { get; private set; }

        public bool IsWaitingForApproval { get; private set; }

        public string Description { get; set; }

        public bool IsRunning { get; set; }

        public bool IsPaused { get; set; }

        public string Period { get; set; }

        public string CronExpression { get; set; }

        public bool IsExecutionGraphEmpty { get; set; }

        public Variable[] LocalVariables { get; set; }

        public string StartedOn { get; set; }

        public int RetryCount { get; set; }

        public int RetryTimeout { get; set; }

        public WorkflowInfo(string dbId,
            int id,
            Guid instanceId,
            string name,
            string filePath,
            LaunchType launchType,
            bool isEnabled,
            bool isApproval,
            bool enableParallelJobs,
            bool isWaitingForApproval,
            string desc,
            bool isRunning,
            bool isPaused,
            string period,
            string cronExpression,
            bool isExecutionGraphEmpty,
            Variable[] localVariables,
            string startedOn,
            int retryCount,
            int retryTimeout)
        {
            DbId = dbId;
            Id = id;
            InstanceId = instanceId;
            Name = name;
            FilePath = filePath;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            IsApproval = isApproval;
            EnableParallelJobs = enableParallelJobs;
            IsWaitingForApproval = isWaitingForApproval;
            Description = desc;
            IsRunning = isRunning;
            IsPaused = isPaused;
            Period = period;
            CronExpression = cronExpression;
            IsExecutionGraphEmpty = isExecutionGraphEmpty;
            LocalVariables = localVariables;
            StartedOn = startedOn;
            RetryCount = retryCount;
            RetryTimeout = retryTimeout;
        }

        public int CompareTo(WorkflowInfo other) => other.Id.CompareTo(Id);
    }
}
