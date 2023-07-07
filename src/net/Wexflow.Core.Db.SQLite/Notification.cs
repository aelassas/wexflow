namespace Wexflow.Core.Db.SQLite
{
    public class Notification : Core.Db.Notification
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameAssignedBy = "ASSIGNED_BY";
        public const string ColumnNameAssignedOn = "ASSIGNED_ON";
        public const string ColumnNameAssignedTo = "ASSIGNED_TO";
        public const string ColumnNameMessage = "MESSAGE";
        public const string ColumnNameIsRead = "IS_READ";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnNameAssignedBy + " INTEGER, "
                                                        + ColumnNameAssignedOn + " TEXT, "
                                                        + ColumnNameAssignedTo + " INTEGER, "
                                                        + ColumnNameMessage + " TEXT, "
                                                        + ColumnNameIsRead + " INTEGER)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
