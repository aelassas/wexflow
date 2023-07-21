namespace Wexflow.Core.Db.SQLite
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_WORKFLOW_ID = "WORKFLOW_ID";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + COLUMN_NAME_USER_ID + " INTEGER, " + COLUMN_NAME_WORKFLOW_ID + " INTEGER)";

        public long Id { get; set; }
    }
}
