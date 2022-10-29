namespace Wexflow.Core.Db.SQLite
{
    public class Workflow : Core.Db.Workflow
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Xml = "XML";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_Xml + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
