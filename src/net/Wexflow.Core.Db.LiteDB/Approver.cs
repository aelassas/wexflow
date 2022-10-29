using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Approver : Core.Db.Approver
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
