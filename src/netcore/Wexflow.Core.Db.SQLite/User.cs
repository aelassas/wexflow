namespace Wexflow.Core.Db.SQLite
{
    public class User : Core.Db.User
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_USERNAME = "USERNAME";
        public const string COLUMN_NAME_PASSWORD = "PASSWORD";
        public const string COLUMN_NAME_USER_PROFILE = "USER_PROFILE";
        public const string COLUMN_NAME_EMAIL = "EMAIL";
        public const string COLUMN_NAME_CREATED_ON = "CREATED_ON";
        public const string COLUMN_NAME_MODIFIED_ON = "MODIFIED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + COLUMN_NAME_USERNAME + " TEXT, " + COLUMN_NAME_PASSWORD + " TEXT, " + COLUMN_NAME_USER_PROFILE + " INTEGER, " + COLUMN_NAME_EMAIL + " TEXT, " + COLUMN_NAME_CREATED_ON + " TEXT, " + COLUMN_NAME_MODIFIED_ON + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
