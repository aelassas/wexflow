namespace Wexflow.Core.Db.SQLServer
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_WorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT)";

        public string Id { get; set; }
    }
}
