using System;

namespace Wexflow.Core.Db
{
    public class Notification
    {
        public const string DOCUMENT_NAME = "notifications";

        public string AssignedBy { get; set; }
        public DateTime AssignedOn { get; set; }
        public string AssignedTo { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
