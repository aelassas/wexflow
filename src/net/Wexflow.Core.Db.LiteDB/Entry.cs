using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Entry : Core.Db.Entry
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
