namespace Wexflow.Core.Db.RavenDB
{
    public class Version : Core.Db.Version
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
