namespace Wexflow.Core.Db.SQLite
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_Xml = "XML";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnName_Xml + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
