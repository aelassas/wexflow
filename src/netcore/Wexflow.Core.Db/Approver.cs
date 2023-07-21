using System;

namespace Wexflow.Core.Db
{
    public class Approver
    {
        public const string DOCUMENT_NAME = "approvers";

        public string UserId { get; set; }
        public string RecordId { get; set; }
        public bool Approved { get; set; }
        public DateTime? ApprovedOn { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
