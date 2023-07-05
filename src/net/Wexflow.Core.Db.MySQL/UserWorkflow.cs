namespace Wexflow.Core.Db.MySQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_WORKFLOW_ID = "WORKFLOW_ID";

        public const string TableStruct = "(" + COLUMN_NAME_ID + " INT NOT NULL AUTO_INCREMENT, " + COLUMN_NAME_USER_ID + " INT, " + COLUMN_NAME_WORKFLOW_ID + " INT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + COLUMN_NAME_ID + "))";

        public string Id { get; set; }
    }
}
