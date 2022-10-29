using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class HistoryEntry : Core.Db.HistoryEntry
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
