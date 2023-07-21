namespace Wexflow.Core.Db.PostgreSQL
{
    public class Approver : Core.Db.Approver
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_APPROVED = "APPROVED";
        public const string COLUMN_NAME_APPROVED_ON = "APPROVED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " SERIAL PRIMARY KEY, "
                                                        + COLUMN_NAME_USER_ID + " INT, "
                                                        + COLUMN_NAME_RECORD_ID + " INT, "
                                                        + COLUMN_NAME_APPROVED + " BOOLEAN, "
                                                        + COLUMN_NAME_APPROVED_ON + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
