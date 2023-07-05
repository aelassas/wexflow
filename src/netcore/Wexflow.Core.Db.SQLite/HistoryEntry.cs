namespace Wexflow.Core.Db.SQLite
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

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_Name + " TEXT, " + ColumnName_Description + " TEXT, " + ColumnName_LaunchType + " INTEGER, " + ColumnName_StatusDate + " TEXT, " + ColumnName_Status + " INTEGER, " + ColumnName_WorkflowId + " INTEGER, " + ColumnName_Logs + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
