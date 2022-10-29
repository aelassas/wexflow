using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        [BsonId]
        public int Id { get; set; }
    }
}
