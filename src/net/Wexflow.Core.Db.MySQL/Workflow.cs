namespace Wexflow.Core.Db.MySQL
{
    public class Workflow : Core.Db.Workflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Xml = "XML";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, " + ColumnName_Xml + " LONGTEXT, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
