namespace Wexflow.Core.Db.SQLServer
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_Xml = "XML";

        public const string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, " + ColumnName_Xml + " VARCHAR(MAX))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
