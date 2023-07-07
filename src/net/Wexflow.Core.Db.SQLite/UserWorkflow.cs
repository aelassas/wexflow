namespace Wexflow.Core.Db.SQLite
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnNameUserId + " INTEGER, " + ColumnNameWorkflowId + " INTEGER)";

        public long Id { get; set; }
    }
}
