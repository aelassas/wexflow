namespace Wexflow.Core.Db.MySQL
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_Xml = "XML";

        public const string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_Xml + " LONGTEXT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
