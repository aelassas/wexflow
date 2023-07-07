namespace Wexflow.Core.Db.SQLServer
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

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnNameUsername + " VARCHAR(255), " + ColumnNamePassword + " VARCHAR(255), " + ColumnNameUserProfile + " INT, " + ColumnNameEmail + " VARCHAR(255), " + ColumnNameCreatedOn + " DATETIME, " + ColumnNameModifiedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
