using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wexflow.Core.Db.MongoDB
{
    public class Workflow : Core.Db.Workflow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
