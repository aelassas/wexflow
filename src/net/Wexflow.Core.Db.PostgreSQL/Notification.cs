namespace Wexflow.Core.Db.PostgreSQL
{
    public class Notification : Core.Db.Notification
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_AssignedBy = "ASSIGNED_BY";
        public const string ColumnName_AssignedOn = "ASSIGNED_ON";
        public const string ColumnName_AssignedTo = "ASSIGNED_TO";
        public const string ColumnName_Message = "MESSAGE";
        public const string ColumnName_IsRead = "IS_READ";

        public const string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, "
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
