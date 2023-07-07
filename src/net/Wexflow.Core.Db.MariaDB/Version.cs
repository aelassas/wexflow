namespace Wexflow.Core.Db.MariaDB
{
    public class Version : Core.Db.Version
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameFilePath = "FILE_PATH";
        public const string ColumnNameCreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnNameRecordId + " INT, "
                                                        + ColumnNameFilePath + " LONGTEXT, "
                                                        + ColumnNameCreatedOn + " TIMESTAMP, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
