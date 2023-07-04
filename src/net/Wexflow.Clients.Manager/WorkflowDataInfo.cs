using System;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.Manager
{
    public class WorkflowDataInfo : IComparable
    {
        public int Id { get; }
        public string Name { get; private set; }
        public LaunchType LaunchType { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsApproval { get; private set; }
        public string Description { get; private set; }

        public WorkflowDataInfo(int id, string name, LaunchType launchType, bool isEnabled, bool isApproval, string desc)
        {
            Id = id;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            IsApproval = isApproval;
            Description = desc;
        }

        public int CompareTo(object obj)
        {
            var wf = (WorkflowDataInfo)obj;
            return wf.Id.CompareTo(Id);
        }
    }
}
