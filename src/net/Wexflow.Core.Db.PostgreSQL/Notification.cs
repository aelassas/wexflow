namespace Wexflow.Core.Db.PostgreSQL
{
    public class Notification : Core.Db.Notification
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_AssignedBy = "ASSIGNED_BY";
        public static readonly string ColumnName_AssignedOn = "ASSIGNED_ON";
        public static readonly string ColumnName_AssignedTo = "ASSIGNED_TO";
        public static readonly string ColumnName_Message = "MESSAGE";
        public static readonly string ColumnName_IsRead = "IS_READ";

        public static readonly string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, "
                                                        + ColumnName_AssignedBy + " INT, "
                                                        + ColumnName_AssignedOn + " TIMESTAMP, "
                                                        + ColumnName_AssignedTo + " INT, "
                                                        + ColumnName_Message + " VARCHAR, "
                                                        + ColumnName_IsRead + " BOOLEAN)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
