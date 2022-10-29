using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Notification : Core.Db.Notification
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
