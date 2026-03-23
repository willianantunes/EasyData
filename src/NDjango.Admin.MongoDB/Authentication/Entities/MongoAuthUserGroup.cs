using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NDjango.Admin.MongoDB.Authentication.Entities
{
    [BsonIgnoreExtraElements]
    public class MongoAuthUserGroup
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public ObjectId UserId { get; set; }
        public ObjectId GroupId { get; set; }
    }
}
