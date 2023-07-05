namespace Wexflow.Core.Db.PostgreSQL
{
    public class HistoryEntry : Core.Db.HistoryEntry
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_Name = "NAME";
        public const string ColumnName_Description = "DESCRIPTION";
        public const string ColumnName_LaunchType = "LAUNCH_TYPE";
        public const string ColumnName_StatusDate = "STATUS_DATE";
        public const string ColumnName_Status = "STATUS";
        public const string ColumnName_WorkflowId = "WORKFLOW_ID";
        public const string ColumnName_Logs = "LOGS";

        public const string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, " + ColumnName_Name + " VARCHAR(255), " + ColumnName_Description + " VARCHAR(255), " + ColumnName_LaunchType + " INT, " + ColumnName_StatusDate + " TIMESTAMP, " + ColumnName_Status + " INT, " + ColumnName_WorkflowId + " INT, " + ColumnName_Logs + " VARCHAR)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
