namespace Wexflow.Core.Db.MariaDB
{
    public class Version : Core.Db.Version
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_FilePath = "FILE_PATH";
        public const string ColumnName_CreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_FilePath + " LONGTEXT, "
                                                        + ColumnName_CreatedOn + " TIMESTAMP, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
