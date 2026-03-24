using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NDjango.Admin;

[BsonIgnoreExtraElements]
public abstract class StandardDocument
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class Category : StandardDocument, IAdminSettings<Category>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public PropertyList<Category> SearchFields => new(x => x.Name, x => x.Description);
}

[BsonIgnoreExtraElements]
public class Restaurant : StandardDocument, IAdminSettings<Restaurant>
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public PropertyList<Restaurant> SearchFields => new(x => x.Name);
}

[BsonIgnoreExtraElements]
public class RestaurantProfile : StandardDocument
{
    public ObjectId RestaurantId { get; set; }
    public string Website { get; set; } = "";
    public string OpeningHours { get; set; } = "";
    public int Capacity { get; set; }
}

[BsonIgnoreExtraElements]
public class Ingredient : StandardDocument
{
    public string Name { get; set; } = "";
    public bool IsAllergen { get; set; }
}

[BsonIgnoreExtraElements]
public class MenuItem : StandardDocument
{
    public ObjectId RestaurantId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}

[BsonIgnoreExtraElements]
public class MenuItemIngredient : StandardDocument
{
    public ObjectId MenuItemId { get; set; }
    public ObjectId IngredientId { get; set; }
}

[BsonIgnoreExtraElements]
public class Gift : StandardDocument
{
    public string Name { get; set; } = "";
    public bool IsWrapped { get; set; }
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid TrackingCode { get; set; }
    public decimal Price { get; set; }
    public long Barcode { get; set; }
    public double Weight { get; set; }
    public float Rating { get; set; }
    public short QuantityInStock { get; set; }
    public byte MinAge { get; set; }
    public DateTimeOffset ShippedAt { get; set; }
    public string Description { get; set; } = "";
    public string Notes { get; set; } = "";
}
