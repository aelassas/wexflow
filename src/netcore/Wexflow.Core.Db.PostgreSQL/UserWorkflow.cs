namespace Wexflow.Core.Db.PostgreSQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_WORKFLOW_ID = "WORKFLOW_ID";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " SERIAL PRIMARY KEY, " + COLUMN_NAME_USER_ID + " INT, " + COLUMN_NAME_WORKFLOW_ID + " INT)";

        public string Id { get; set; }
    }
}
