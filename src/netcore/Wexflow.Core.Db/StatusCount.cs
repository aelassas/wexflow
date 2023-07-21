namespace Wexflow.Core.Db
{
    public class StatusCount
    {
        public const string DOCUMENT_NAME = "statusCount";

        public int PendingCount { get; set; }
        public int RunningCount { get; set; }
        public int DoneCount { get; set; }
        public int FailedCount { get; set; }
        public int WarningCount { get; set; }
        public int DisabledCount { get; set; }
        public int StoppedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}