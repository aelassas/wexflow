using System;

namespace Wexflow.Core.Db
{
    public class Record
    {
        public const string DOCUMENT_NAME = "records";

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comments { get; set; }
        public bool Approved { get; set; }
        public string ManagerComments { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? AssignedOn { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
