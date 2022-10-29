using LiteDB;

namespace Wexflow.Core.Db.LiteDB
{
    public class Workflow : Core.Db.Workflow
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
