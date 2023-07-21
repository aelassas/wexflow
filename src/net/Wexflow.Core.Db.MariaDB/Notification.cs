namespace Wexflow.Core.Db.MariaDB
{
    public class Notification : Core.Db.Notification
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_ASSIGNED_BY = "ASSIGNED_BY";
        public const string COLUMN_NAME_ASSIGNED_ON = "ASSIGNED_ON";
        public const string COLUMN_NAME_ASSIGNED_TO = "ASSIGNED_TO";
        public const string COLUMN_NAME_MESSAGE = "MESSAGE";
        public const string COLUMN_NAME_IS_READ = "IS_READ";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT NOT NULL AUTO_INCREMENT, "
                                                        + COLUMN_NAME_ASSIGNED_BY + " INT, "
                                                        + COLUMN_NAME_ASSIGNED_ON + " TIMESTAMP, "
                                                        + COLUMN_NAME_ASSIGNED_TO + " INT, "
                                                        + COLUMN_NAME_MESSAGE + " LONGTEXT, "
                                                        + COLUMN_NAME_IS_READ + " BIT(1), CONSTRAINT " + DOCUMENT_NAME + "_pk PRIMARY KEY (" + COLUMN_NAME_ID + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
