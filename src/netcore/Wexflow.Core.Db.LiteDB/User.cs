using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class User : Core.Db.User
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
