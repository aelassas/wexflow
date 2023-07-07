namespace Wexflow.Core.Db.SQLServer
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameXml = "XML";

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnNameXml + " VARCHAR(MAX))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
