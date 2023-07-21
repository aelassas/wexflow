using System;

namespace Wexflow.Core.Db
{
    public class HistoryEntry
    {
        public const string DOCUMENT_NAME = "historyEntries";

        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public LaunchType LaunchType { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string Logs { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
