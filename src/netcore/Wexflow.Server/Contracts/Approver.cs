namespace Wexflow.Server.Contracts
{
    public class Approver
    {
        public string ApprovedBy { get; set; }
        public bool Approved { get; set; }
        public string ApprovedOn { get; set; }
    }
}
