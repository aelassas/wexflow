namespace Wexflow.Core.Db.SQLite
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_WorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_UserId + " INTEGER, " + ColumnName_WorkflowId + " INTEGER)";

        public long Id { get; set; }
    }
}
