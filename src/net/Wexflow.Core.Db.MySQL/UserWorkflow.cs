namespace Wexflow.Core.Db.MySQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameWorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, " + ColumnNameUserId + " INT, " + ColumnNameWorkflowId + " INT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public string Id { get; set; }
    }
}
