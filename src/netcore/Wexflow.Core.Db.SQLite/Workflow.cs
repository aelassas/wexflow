namespace Wexflow.Core.Db.SQLite
{
    public class Workflow : Core.Db.Workflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_XML = "XML";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " + COLUMN_NAME_XML + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
