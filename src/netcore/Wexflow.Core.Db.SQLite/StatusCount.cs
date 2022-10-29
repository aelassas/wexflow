namespace Wexflow.Core.Db.SQLite
{
    public class StatusCount : Core.Db.StatusCount
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_PendingCount = "PENDING_COUNT";
        public static readonly string ColumnName_RunningCount = "RUNNING_COUNT";
        public static readonly string ColumnName_DoneCount = "DONE_COUNT";
        public static readonly string ColumnName_FailedCount = "FAILED_COUNT";
        public static readonly string ColumnName_WarningCount = "WARNING_COUNT";
        public static readonly string ColumnName_DisabledCount = "DISABLED_COUNT";
        public static readonly string ColumnName_StoppedCount = "STOPPED_COUNT";
        public static readonly string ColumnName_RejectedCount = "DISAPPROVED_COUNT";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_PendingCount + " INTEGER, " + ColumnName_RunningCount + " INTEGER, " + ColumnName_DoneCount + " INTEGER, " + ColumnName_FailedCount + " INTEGER, " + ColumnName_WarningCount + " INTEGER, " + ColumnName_DisabledCount + " INTEGER, " + ColumnName_StoppedCount + " INTEGER, " + ColumnName_RejectedCount + " INTEGER)";

        public long Id { get; set; }
    }
}