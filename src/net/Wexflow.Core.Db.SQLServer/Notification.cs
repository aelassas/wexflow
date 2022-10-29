namespace Wexflow.Core.Db.SQLServer
{
    public class Notification : Core.Db.Notification
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_AssignedBy = "ASSIGNED_BY";
        public static readonly string ColumnName_AssignedOn = "ASSIGNED_ON";
        public static readonly string ColumnName_AssignedTo = "ASSIGNED_TO";
        public static readonly string ColumnName_Message = "MESSAGE";
        public static readonly string ColumnName_IsRead = "IS_READ";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnName_AssignedBy + " INT, "
                                                        + ColumnName_AssignedOn + " DATETIME, "
                                                        + ColumnName_AssignedTo + " INT, "
                                                        + ColumnName_Message + " VARCHAR(MAX), "
                                                        + ColumnName_IsRead + " BIT)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
