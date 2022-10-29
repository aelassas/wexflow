namespace Wexflow.Core.Db.MariaDB
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_UserId + " INT, " + ColumnName_WorkflowId + " INT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public string Id { get; set; }
    }
}
