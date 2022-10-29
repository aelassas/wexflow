namespace Wexflow.Core.Db.SQLite
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_UserId + " INTEGER, " + ColumnName_WorkflowId + " INTEGER)";

        public long Id { get; set; }
    }
}
