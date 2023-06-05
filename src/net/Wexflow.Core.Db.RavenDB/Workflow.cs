namespace Wexflow.Core.Db.RavenDB
{
    public class Workflow : Core.Db.Workflow
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
