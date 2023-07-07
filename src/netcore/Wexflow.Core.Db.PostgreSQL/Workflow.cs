namespace Wexflow.Core.Db.PostgreSQL
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameXml = "XML";

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, " + ColumnNameXml + " XML)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
