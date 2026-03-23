using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NDjango.Admin.MongoDB.Authentication.Entities
{
    [BsonIgnoreExtraElements]
    public class MongoAuthUser
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsSuperuser { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }
}
