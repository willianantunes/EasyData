using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NDjango.Admin.MongoDB.Authentication.Entities
{
    [BsonIgnoreExtraElements]
    public class MongoAuthGroup
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; }
    }
}
