namespace Wexflow.Core.Db.PostgreSQL
{
    public class Workflow : Core.Db.Workflow
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_XML = "XML";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " SERIAL PRIMARY KEY, " + COLUMN_NAME_XML + " XML)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
