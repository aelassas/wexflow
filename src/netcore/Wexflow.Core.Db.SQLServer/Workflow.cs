namespace Wexflow.Core.Db.SQLServer
{
    public class Workflow : Core.Db.Workflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Xml = "XML";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnName_Xml + " VARCHAR(MAX))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
