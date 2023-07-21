namespace Wexflow.Core.Db.MariaDB
{
    public class Workflow : Core.Db.Workflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_XML = "XML";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INT NOT NULL AUTO_INCREMENT, " + COLUMN_NAME_XML + " LONGTEXT, CONSTRAINT " + DOCUMENT_NAME + "_pk PRIMARY KEY (" + COLUMN_NAME_ID + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
