namespace Wexflow.Core.Db.RavenDB
{
    public class Notification : Core.Db.Notification
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
