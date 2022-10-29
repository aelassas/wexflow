namespace Wexflow.Core.Db.SQLite
{
    public class Notification : Core.Db.Notification
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_AssignedBy = "ASSIGNED_BY";
        public static readonly string ColumnName_AssignedOn = "ASSIGNED_ON";
        public static readonly string ColumnName_AssignedTo = "ASSIGNED_TO";
        public static readonly string ColumnName_Message = "MESSAGE";
        public static readonly string ColumnName_IsRead = "IS_READ";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
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
