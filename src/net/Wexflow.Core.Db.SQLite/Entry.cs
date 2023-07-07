namespace Wexflow.Core.Db.SQLite
{
    public class Entry : Core.Db.Entry
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameName = "NAME";
        public const string ColumnNameDescription = "DESCRIPTION";
        public const string ColumnNameLaunchType = "LAUNCH_TYPE";
        public const string ColumnNameStatusDate = "STATUS_DATE";
        public const string ColumnNameStatus = "STATUS";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";
        public const string ColumnNameJobId = "JOB_ID";
        public const string ColumnNameLogs = "LOGS";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnNameName + " TEXT, " + ColumnNameDescription + " TEXT, " + ColumnNameLaunchType + " INTEGER, " + ColumnNameStatusDate + " TEXT, " + ColumnNameStatus + " INTEGER, " + ColumnNameWorkflowId + " INTEGER, " + ColumnNameJobId + " TEXT, " + ColumnNameLogs + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
