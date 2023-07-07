namespace Wexflow.Core.Db.SQLServer
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnNameUserId + " INT, " + ColumnNameWorkflowId + " INT)";

        public string Id { get; set; }
    }
}
