namespace Wexflow.Core.Db.SQLite
{
    public class Entry : Core.Db.Entry
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_NAME = "NAME";
        public const string COLUMN_NAME_DESCRIPTION = "DESCRIPTION";
        public const string COLUMN_NAME_LAUNCH_TYPE = "LAUNCH_TYPE";
        public const string COLUMN_NAME_STATUS_DATE = "STATUS_DATE";
        public const string COLUMN_NAME_STATUS = "STATUS";
        public const string COLUMN_NAME_WORKFLOW_ID = "WORKFLOW_ID";
        public const string COLUMN_NAME_JOB_ID = "JOB_ID";
        public const string COLUMN_NAME_LOGS = "LOGS";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + COLUMN_NAME_NAME + " TEXT, " + COLUMN_NAME_DESCRIPTION + " TEXT, " + COLUMN_NAME_LAUNCH_TYPE + " INTEGER, " + COLUMN_NAME_STATUS_DATE + " TEXT, " + COLUMN_NAME_STATUS + " INTEGER, " + COLUMN_NAME_WORKFLOW_ID + " INTEGER, " + COLUMN_NAME_JOB_ID + " TEXT, " + COLUMN_NAME_LOGS + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
