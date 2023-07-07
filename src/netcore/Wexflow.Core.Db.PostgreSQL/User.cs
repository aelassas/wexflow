namespace Wexflow.Core.Db.PostgreSQL
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

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, " + ColumnNameUsername + " VARCHAR(255), " + ColumnNamePassword + " VARCHAR(255), " + ColumnNameUserProfile + " INT, " + ColumnNameEmail + " VARCHAR(255), " + ColumnNameCreatedOn + " TIMESTAMP, " + ColumnNameModifiedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
