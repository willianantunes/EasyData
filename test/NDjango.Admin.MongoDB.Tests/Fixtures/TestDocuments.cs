using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using NDjango.Admin;

namespace NDjango.Admin.MongoDB.Tests.Fixtures
{
    [BsonIgnoreExtraElements]
    public class TestDocumentWithCreatedDate
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class TestCategory : IAdminSettings<TestCategory>
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public PropertyList<TestCategory> SearchFields => new(x => x.Name, x => x.Description);
    }

    [BsonIgnoreExtraElements]
    public class TestRestaurant : IAdminSettings<TestRestaurant>
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public PropertyList<TestRestaurant> SearchFields => new(x => x.Name);
    }

    [BsonIgnoreExtraElements]
    public class TestIngredient : IAdminSettings<TestIngredient>
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; } = "";
        public bool IsAllergen { get; set; }
        public PropertyList<TestIngredient> SearchFields => new(x => x.Name);
    }
}
