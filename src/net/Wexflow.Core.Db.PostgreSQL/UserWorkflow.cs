namespace Wexflow.Core.Db.PostgreSQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";

        public static readonly string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT)";

        public string Id { get; set; }
    }
}
