namespace Wexflow.Core.Db.SQLite
{
    public class HistoryEntry : Core.Db.HistoryEntry
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Name = "NAME";
        public static readonly string ColumnName_Description = "DESCRIPTION";
        public static readonly string ColumnName_LaunchType = "LAUNCH_TYPE";
        public static readonly string ColumnName_StatusDate = "STATUS_DATE";
        public static readonly string ColumnName_Status = "STATUS";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";
        public static readonly string ColumnName_Logs = "LOGS";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_Name + " TEXT, " + ColumnName_Description + " TEXT, " + ColumnName_LaunchType + " INTEGER, " + ColumnName_StatusDate + " TEXT, " + ColumnName_Status + " INTEGER, " + ColumnName_WorkflowId + " INTEGER, " + ColumnName_Logs + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
