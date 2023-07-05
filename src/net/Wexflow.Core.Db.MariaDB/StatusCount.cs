namespace Wexflow.Core.Db.MariaDB
{
    public class StatusCount : Core.Db.StatusCount
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_PendingCount = "PENDING_COUNT";
        public const string ColumnName_RunningCount = "RUNNING_COUNT";
        public const string ColumnName_DoneCount = "DONE_COUNT";
        public const string ColumnName_FailedCount = "FAILED_COUNT";
        public const string ColumnName_WarningCount = "WARNING_COUNT";
        public const string ColumnName_DisabledCount = "DISABLED_COUNT";
        public const string ColumnName_StoppedCount = "STOPPED_COUNT";
        public const string ColumnName_RejectedCount = "DISAPPROVED_COUNT";

        public const string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_PendingCount + " INT, " + ColumnName_RunningCount + " INT, " + ColumnName_DoneCount + " INT, " + ColumnName_FailedCount + " INT, " + ColumnName_WarningCount + " INT, " + ColumnName_DisabledCount + " INT, " + ColumnName_StoppedCount + " INT, " + ColumnName_RejectedCount + " INT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }
    }
}