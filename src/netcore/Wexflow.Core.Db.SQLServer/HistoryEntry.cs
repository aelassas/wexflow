namespace Wexflow.Core.Db.SQLServer
{
    public class HistoryEntry : Core.Db.HistoryEntry
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameName = "NAME";
        public const string ColumnNameDescription = "DESCRIPTION";
        public const string ColumnNameLaunchType = "LAUNCH_TYPE";
        public const string ColumnNameStatusDate = "STATUS_DATE";
        public const string ColumnNameStatus = "STATUS";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";
        public const string ColumnNameLogs = "LOGS";

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnNameName + " VARCHAR(255), " + ColumnNameDescription + " VARCHAR(255), " + ColumnNameLaunchType + " INT, " + ColumnNameStatusDate + " DATETIME, " + ColumnNameStatus + " INT, " + ColumnNameWorkflowId + " INT, " + ColumnNameLogs + " VARCHAR(MAX))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
