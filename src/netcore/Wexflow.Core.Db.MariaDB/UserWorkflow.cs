namespace Wexflow.Core.Db.MariaDB
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_WorkflowId = "WORKFLOW_ID";

        public const string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public string Id { get; set; }
    }
}
