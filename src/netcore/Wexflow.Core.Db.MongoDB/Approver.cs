using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wexflow.Core.Db.MongoDB
{
    public class Approver : Core.Db.Approver
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
