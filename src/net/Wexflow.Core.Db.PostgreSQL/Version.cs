namespace Wexflow.Core.Db.PostgreSQL
{
    public class Version : Core.Db.Version
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameFilePath = "FILE_PATH";
        public const string ColumnNameCreatedOn = "CREATED_ON";

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, "
                                                        + ColumnNameRecordId + " INT, "
                                                        + ColumnNameFilePath + " VARCHAR, "
                                                        + ColumnNameCreatedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
