namespace Wexflow.Core.Db.MariaDB
{
    public class Version : Core.Db.Version
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_RECORD_ID = "RECORD_ID";
        public const string COLUMN_NAME_FILE_PATH = "FILE_PATH";
        public const string COLUMN_NAME_CREATED_ON = "CREATED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT NOT NULL AUTO_INCREMENT, "
                                                        + COLUMN_NAME_RECORD_ID + " INT, "
                                                        + COLUMN_NAME_FILE_PATH + " LONGTEXT, "
                                                        + COLUMN_NAME_CREATED_ON + " TIMESTAMP, CONSTRAINT " + DOCUMENT_NAME + "_pk PRIMARY KEY (" + COLUMN_NAME_ID + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
