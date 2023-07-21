namespace Wexflow.Core.Db.SQLServer
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_WORKFLOW_ID = "WORKFLOW_ID";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, " + COLUMN_NAME_USER_ID + " INT, " + COLUMN_NAME_WORKFLOW_ID + " INT)";

        public string Id { get; set; }
    }
}
