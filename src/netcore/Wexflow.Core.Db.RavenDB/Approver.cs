namespace Wexflow.Core.Db.RavenDB
{
    public class Approver : Core.Db.Approver
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
