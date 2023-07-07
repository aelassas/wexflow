namespace Wexflow.Core.Db.MariaDB
{
    public class Notification : Core.Db.Notification
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameAssignedBy = "ASSIGNED_BY";
        public const string ColumnNameAssignedOn = "ASSIGNED_ON";
        public const string ColumnNameAssignedTo = "ASSIGNED_TO";
        public const string ColumnNameMessage = "MESSAGE";
        public const string ColumnNameIsRead = "IS_READ";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnNameAssignedBy + " INT, "
                                                        + ColumnNameAssignedOn + " TIMESTAMP, "
                                                        + ColumnNameAssignedTo + " INT, "
                                                        + ColumnNameMessage + " LONGTEXT, "
                                                        + ColumnNameIsRead + " BIT(1), CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
