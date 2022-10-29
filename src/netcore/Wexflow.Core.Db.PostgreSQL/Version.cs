namespace Wexflow.Core.Db.PostgreSQL
{
    public class Version : Core.Db.Version
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_RecordId = "RECORD_ID";
        public static readonly string ColumnName_FilePath = "FILE_PATH";
        public static readonly string ColumnName_CreatedOn = "CREATED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_FilePath + " VARCHAR, "
                                                        + ColumnName_CreatedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
