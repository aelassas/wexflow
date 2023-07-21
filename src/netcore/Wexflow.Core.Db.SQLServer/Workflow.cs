namespace Wexflow.Core.Db.SQLServer
{
    public class Workflow : Core.Db.Workflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_XML = "XML";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT IDENTITY(1,1) PRIMARY KEY, " + COLUMN_NAME_XML + " VARCHAR(MAX))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
