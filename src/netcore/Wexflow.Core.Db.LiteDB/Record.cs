using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Record : Core.Db.Record
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
