namespace Wexflow.Core.Db.SQLServer
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT)";

        public string Id { get; set; }
    }
}
