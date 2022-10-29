namespace Wexflow.Core.Db.MariaDB
{
    public class Entry : Core.Db.Entry
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Name = "NAME";
        public static readonly string ColumnName_Description = "DESCRIPTION";
        public static readonly string ColumnName_LaunchType = "LAUNCH_TYPE";
        public static readonly string ColumnName_StatusDate = "STATUS_DATE";
        public static readonly string ColumnName_Status = "STATUS";
        public static readonly string ColumnName_WorkflowId = "WORKFLOW_ID";
        public static readonly string ColumnName_JobId = "JOB_ID";
        public static readonly string ColumnName_Logs = "LOGS";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_Name + " VARCHAR(255), " + ColumnName_Description + " VARCHAR(255), " + ColumnName_LaunchType + " INT, " + ColumnName_StatusDate + " TIMESTAMP, " + ColumnName_Status + " INT, " + ColumnName_WorkflowId + " INT, " + ColumnName_JobId + " VARCHAR(255), " + ColumnName_Logs + " LONGTEXT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
