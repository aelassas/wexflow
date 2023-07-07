namespace Wexflow.Core.Db.PostgreSQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, " + ColumnNameUserId + " INT, " + ColumnNameWorkflowId + " INT)";

        public string Id { get; set; }
    }
}
