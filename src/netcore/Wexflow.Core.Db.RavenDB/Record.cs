namespace Wexflow.Core.Db.RavenDB
{
    public class Record : Core.Db.Record
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
