using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wexflow.Core.Db.MongoDB
{
    public class Version : Core.Db.Version
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
