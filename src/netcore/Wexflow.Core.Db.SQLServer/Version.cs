namespace Wexflow.Core.Db.SQLServer
{
    public class Version : Core.Db.Version
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_FILE_PATH = "FILE_PATH";
        public const string COLUMN_NAME_CREATED_ON = "CREATED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + COLUMN_NAME_RECORD_ID + " INT, "
                                                        + COLUMN_NAME_FILE_PATH + " VARCHAR(1024), "
                                                        + COLUMN_NAME_CREATED_ON + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
