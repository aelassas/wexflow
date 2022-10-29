namespace Wexflow.Core.Db.SQLServer
{
    public class Version : Core.Db.Version
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_RecordId = "RECORD_ID";
        public static readonly string ColumnName_FilePath = "FILE_PATH";
        public static readonly string ColumnName_CreatedOn = "CREATED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, "
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
