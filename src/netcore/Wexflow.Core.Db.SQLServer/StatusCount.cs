namespace Wexflow.Core.Db.SQLServer
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

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnNamePendingCount + " INT, " + ColumnNameRunningCount + " INT, " + ColumnNameDoneCount + " INT, " + ColumnNameFailedCount + " INT, " + ColumnNameWarningCount + " INT, " + ColumnNameDisabledCount + " INT, " + ColumnNameStoppedCount + " INT, " + ColumnNameRejectedCount + " INT)";

        public int Id { get; set; }
    }
}