using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Version : Core.Db.Version
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
