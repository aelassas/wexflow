namespace Wexflow.Core.Db.SQLite
{
    public class Version : Core.Db.Version
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameFilePath = "FILE_PATH";
        public const string ColumnNameCreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnNameRecordId + " INTEGER, "
                                                        + ColumnNameFilePath + " TEXT, "
                                                        + ColumnNameCreatedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
