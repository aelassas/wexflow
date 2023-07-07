namespace Wexflow.Core.Db.SQLServer
{
    public class Notification : Core.Db.Notification
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameAssignedBy = "ASSIGNED_BY";
        public const string ColumnNameAssignedOn = "ASSIGNED_ON";
        public const string ColumnNameAssignedTo = "ASSIGNED_TO";
        public const string ColumnNameMessage = "MESSAGE";
        public const string ColumnNameIsRead = "IS_READ";

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnNameAssignedBy + " INT, "
                                                        + ColumnNameAssignedOn + " DATETIME, "
                                                        + ColumnNameAssignedTo + " INT, "
                                                        + ColumnNameMessage + " VARCHAR(MAX), "
                                                        + ColumnNameIsRead + " BIT)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
