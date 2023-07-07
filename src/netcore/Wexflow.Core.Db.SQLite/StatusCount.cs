namespace Wexflow.Core.Db.SQLite
{
    public class StatusCount : Core.Db.StatusCount
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNamePendingCount = "PENDING_COUNT";
        public const string ColumnNameRunningCount = "RUNNING_COUNT";
        public const string ColumnNameDoneCount = "DONE_COUNT";
        public const string ColumnNameFailedCount = "FAILED_COUNT";
        public const string ColumnNameWarningCount = "WARNING_COUNT";
        public const string ColumnNameDisabledCount = "DISABLED_COUNT";
        public const string ColumnNameStoppedCount = "STOPPED_COUNT";
        public const string ColumnNameRejectedCount = "DISAPPROVED_COUNT";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnNamePendingCount + " INTEGER, " + ColumnNameRunningCount + " INTEGER, " + ColumnNameDoneCount + " INTEGER, " + ColumnNameFailedCount + " INTEGER, " + ColumnNameWarningCount + " INTEGER, " + ColumnNameDisabledCount + " INTEGER, " + ColumnNameStoppedCount + " INTEGER, " + ColumnNameRejectedCount + " INTEGER)";

        public long Id { get; set; }
    }
}