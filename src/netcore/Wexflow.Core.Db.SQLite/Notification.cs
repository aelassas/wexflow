namespace Wexflow.Core.Db.SQLite
{
    public class Notification : Core.Db.Notification
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_AssignedBy = "ASSIGNED_BY";
        public const string ColumnName_AssignedOn = "ASSIGNED_ON";
        public const string ColumnName_AssignedTo = "ASSIGNED_TO";
        public const string ColumnName_Message = "MESSAGE";
        public const string ColumnName_IsRead = "IS_READ";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnName_AssignedBy + " INTEGER, "
                                                        + ColumnName_AssignedOn + " TEXT, "
                                                        + ColumnName_AssignedTo + " INTEGER, "
                                                        + ColumnName_Message + " TEXT, "
                                                        + ColumnName_IsRead + " INTEGER)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
