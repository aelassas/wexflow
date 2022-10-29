using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class StatusCount : Core.Db.StatusCount
    {
        [BsonId]
        public int Id { get; set; }
    }
}