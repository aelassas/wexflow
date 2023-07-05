namespace Wexflow.Core.Db.PostgreSQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_WorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT)";

        public string Id { get; set; }
    }
}
