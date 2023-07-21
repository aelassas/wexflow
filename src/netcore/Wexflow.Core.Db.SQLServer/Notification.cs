namespace Wexflow.Core.Db.SQLServer
{
    public class Notification : Core.Db.Notification
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_ASSIGNED_BY = "ASSIGNED_BY";
        public const string COLUMN_NAME_ASSIGNED_ON = "ASSIGNED_ON";
        public const string COLUMN_NAME_ASSIGNED_TO = "ASSIGNED_TO";
        public const string COLUMN_NAME_MESSAGE = "MESSAGE";
        public const string COLUMN_NAME_IS_READ = "IS_READ";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + COLUMN_NAME_ASSIGNED_BY + " INT, "
                                                        + COLUMN_NAME_ASSIGNED_ON + " DATETIME, "
                                                        + COLUMN_NAME_ASSIGNED_TO + " INT, "
                                                        + COLUMN_NAME_MESSAGE + " VARCHAR(MAX), "
                                                        + COLUMN_NAME_IS_READ + " BIT)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
