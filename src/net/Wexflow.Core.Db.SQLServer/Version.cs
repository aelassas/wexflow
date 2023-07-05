namespace Wexflow.Core.Db.SQLServer
{
    public class Version : Core.Db.Version
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_FilePath = "FILE_PATH";
        public const string ColumnName_CreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_FilePath + " VARCHAR(1024), "
                                                        + ColumnName_CreatedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
