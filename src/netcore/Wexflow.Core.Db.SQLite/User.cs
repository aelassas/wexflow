namespace Wexflow.Core.Db.SQLite
{
    public class User : Core.Db.User
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUsername = "USERNAME";
        public const string ColumnNamePassword = "PASSWORD";
        public const string ColumnNameUserProfile = "USER_PROFILE";
        public const string ColumnNameEmail = "EMAIL";
        public const string ColumnNameCreatedOn = "CREATED_ON";
        public const string ColumnNameModifiedOn = "MODIFIED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnNameUsername + " TEXT, " + ColumnNamePassword + " TEXT, " + ColumnNameUserProfile + " INTEGER, " + ColumnNameEmail + " TEXT, " + ColumnNameCreatedOn + " TEXT, " + ColumnNameModifiedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
