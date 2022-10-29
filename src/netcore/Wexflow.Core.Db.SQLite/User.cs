namespace Wexflow.Core.Db.SQLite
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

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_Username + " TEXT, " + ColumnName_Password + " TEXT, " + ColumnName_UserProfile + " INTEGER, " + ColumnName_Email + " TEXT, " + ColumnName_CreatedOn + " TEXT, " + ColumnName_ModifiedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
