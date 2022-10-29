namespace Wexflow.Core.Db.PostgreSQL
{
    public class Workflow : Core.Db.Workflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Xml = "XML";

        public static readonly string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, " + ColumnName_Xml + " XML)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
