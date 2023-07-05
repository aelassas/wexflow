namespace Wexflow.Core.Db.SQLite
{
    public class Version : Core.Db.Version
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_FilePath = "FILE_PATH";
        public const string ColumnName_CreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnName_RecordId + " INTEGER, "
                                                        + ColumnName_FilePath + " TEXT, "
                                                        + ColumnName_CreatedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
