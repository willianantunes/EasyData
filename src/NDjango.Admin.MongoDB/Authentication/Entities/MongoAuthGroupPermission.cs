using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NDjango.Admin.MongoDB.Authentication.Entities
{
    [BsonIgnoreExtraElements]
    public class MongoAuthGroupPermission
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public ObjectId GroupId { get; set; }
        public ObjectId PermissionId { get; set; }
    }
}
