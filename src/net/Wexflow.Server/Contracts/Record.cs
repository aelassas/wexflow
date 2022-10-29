namespace Wexflow.Server.Contracts
{
    public class Record
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Comments { get; set; }
        public bool Approved { get; set; }
        public string ManagerComments { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedOn { get; set; }
        public Approver[] Approvers { get; set; }
        public Version[] Versions { get; set; }
    }
}
