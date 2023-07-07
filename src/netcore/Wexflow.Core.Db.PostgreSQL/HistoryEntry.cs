namespace Wexflow.Core.Db.PostgreSQL
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

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, " + ColumnNameName + " VARCHAR(255), " + ColumnNameDescription + " VARCHAR(255), " + ColumnNameLaunchType + " INT, " + ColumnNameStatusDate + " TIMESTAMP, " + ColumnNameStatus + " INT, " + ColumnNameWorkflowId + " INT, " + ColumnNameLogs + " VARCHAR)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
