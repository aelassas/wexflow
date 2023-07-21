using System;

namespace Wexflow.Core.Db
{
    public class Version
    {
        public const string DOCUMENT_NAME = "versions";

        public string FilePath { get; set; }
        public string RecordId { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
