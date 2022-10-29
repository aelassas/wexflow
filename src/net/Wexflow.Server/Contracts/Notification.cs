namespace Wexflow.Server.Contracts
{
    public class Notification
    {
        public string Id { get; set; }
        public string AssignedBy { get; set; }
        public string AssignedOn { get; set; }
        public string AssignedTo { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
    }
}
