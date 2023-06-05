using System;
using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic,
        Cron
    }

    [DataContract]
    public class WorkflowInfo : IComparable
    {
        [DataMember]
        public string DbId { get; private set; }
        [DataMember]
        public int Id { get; private set; }
        [DataMember]
        public Guid InstanceId { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public LaunchType LaunchType { get; private set; }
        [DataMember]
        public bool IsEnabled { get; private set; }
        [DataMember]
        public bool IsApproval { get; private set; }
        [DataMember]
        public bool IsWaitingForApproval { get; set; }
        [DataMember]
        public string Description { get; private set; }
        [DataMember]
        public bool IsRunning { get; set; }
        [DataMember]
        public bool IsPaused { get; set; }
        [DataMember]
        public string Period { get; set; }
        [DataMember]
        public string CronExpression { get; private set; }
        [DataMember]
        public bool IsExecutionGraphEmpty { get; set; }
        [DataMember]
        public Variable[] LocalVariables { get; set; }

        public WorkflowInfo(string dbId, int id, Guid instanceId, string name, LaunchType launchType, bool isEnabled, bool isApproval, bool isWaitingForApproval, string desc, bool isRunning, bool isPaused, string period, string cronExpression, bool isExecutionGraphEmpty, Variable[] localVariables)
        {
            DbId = dbId;
            Id = id;
            InstanceId = instanceId;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            IsApproval = isApproval;
            IsWaitingForApproval = isWaitingForApproval;
            Description = desc;
            IsRunning = isRunning;
            IsPaused = isPaused;
            Period = period;
            CronExpression = cronExpression;
            IsExecutionGraphEmpty = isExecutionGraphEmpty;
            LocalVariables = localVariables;
        }

        public int CompareTo(object obj)
        {
            var wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(Id);
        }
    }
}
