namespace Wexflow.Core.Db.SQLite
{
    public class Version : Core.Db.Version
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_FILE_PATH = "FILE_PATH";
        public const string COLUMN_NAME_CREATED_ON = "CREATED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + COLUMN_NAME_RECORD_ID + " INTEGER, "
                                                        + COLUMN_NAME_FILE_PATH + " TEXT, "
                                                        + COLUMN_NAME_CREATED_ON + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
