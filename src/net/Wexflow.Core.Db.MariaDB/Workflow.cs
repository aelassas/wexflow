namespace Wexflow.Core.Db.MariaDB
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameXml = "XML";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, " + ColumnNameXml + " LONGTEXT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
