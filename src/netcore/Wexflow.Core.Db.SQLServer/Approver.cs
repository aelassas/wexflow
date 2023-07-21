namespace Wexflow.Core.Db.SQLServer
{
    public class Approver : Core.Db.Approver
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_APPROVED = "APPROVED";
        public const string COLUMN_NAME_APPROVED_ON = "APPROVED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + COLUMN_NAME_USER_ID + " INT, "
                                                        + COLUMN_NAME_RECORD_ID + " INT, "
                                                        + COLUMN_NAME_APPROVED + " BIT, "
                                                        + COLUMN_NAME_APPROVED_ON + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
