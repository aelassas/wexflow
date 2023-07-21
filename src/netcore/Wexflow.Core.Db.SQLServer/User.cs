namespace Wexflow.Core.Db.SQLServer
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

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, " + COLUMN_NAME_USERNAME + " VARCHAR(255), " + COLUMN_NAME_PASSWORD + " VARCHAR(255), " + COLUMN_NAME_USER_PROFILE + " INT, " + COLUMN_NAME_EMAIL + " VARCHAR(255), " + COLUMN_NAME_CREATED_ON + " DATETIME, " + COLUMN_NAME_MODIFIED_ON + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
