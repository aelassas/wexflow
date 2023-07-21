namespace Wexflow.Core.Db.SQLite
{
    public class Approver : Core.Db.Approver
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USER_ID = "USER_ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_APPROVED = "APPROVED";
        public const string COLUMN_NAME_APPROVED_ON = "APPROVED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + COLUMN_NAME_USER_ID + " INTEGER, "
                                                        + COLUMN_NAME_RECORD_ID + " INTEGER, "
                                                        + COLUMN_NAME_APPROVED + " INTEGER, "
                                                        + COLUMN_NAME_APPROVED_ON + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
