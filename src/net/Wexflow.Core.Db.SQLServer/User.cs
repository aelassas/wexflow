namespace Wexflow.Core.Db.SQLServer
{
    public class User : Core.Db.User
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Username = "USERNAME";
        public static readonly string ColumnName_Password = "PASSWORD";
        public static readonly string ColumnName_UserProfile = "USER_PROFILE";
        public static readonly string ColumnName_Email = "EMAIL";
        public static readonly string ColumnName_CreatedOn = "CREATED_ON";
        public static readonly string ColumnName_ModifiedOn = "MODIFIED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnName_Username + " VARCHAR(255), " + ColumnName_Password + " VARCHAR(255), " + ColumnName_UserProfile + " INT, " + ColumnName_Email + " VARCHAR(255), " + ColumnName_CreatedOn + " DATETIME, " + ColumnName_ModifiedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
