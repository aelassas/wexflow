namespace Wexflow.Core.Db.SQLite
{
    public class Workflow : Core.Db.Workflow
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameXml = "XML";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + ColumnNameXml + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
