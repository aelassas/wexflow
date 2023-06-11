namespace Wexflow.Server.Contracts
{
    public enum Status
    {
        Pending,
        Running,
        Done,
        Failed,
        Warning,
        Disabled,
        Stopped
    }

    public class Entry
    {
        public string Id { get; set; }

        public int WorkflowId { get; set; }

        public string Name { get; set; }

        public LaunchType LaunchType { get; set; }

        public string Description { get; set; }

        public Status Status { get; set; }

        //public double StatusDate { get; set; }
        public string StatusDate { get; set; }
    }
}
