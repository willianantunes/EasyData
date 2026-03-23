using MongoDB.Bson;
using MongoDB.Driver;

public static class DataSeeder
{
    public static async Task SeedAsync(IMongoDatabase database)
    {
        await SeedCategoriesAsync(database);
        var (restaurant1Id, restaurant2Id) = await SeedRestaurantsAsync(database);
        await SeedRestaurantProfilesAsync(database, restaurant1Id, restaurant2Id);
        var ingredientIds = await SeedIngredientsAsync(database);
        await SeedMenuItemsAsync(database, restaurant1Id, restaurant2Id, ingredientIds);
        await SeedGiftsAsync(database);
    }

    private static async Task SeedCategoriesAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Category>(CollectionNames.Categories);
        if (await collection.CountDocumentsAsync(FilterDefinition<Category>.Empty) > 0)
            return;

        var categories = new List<Category>
        {
            new() { Name = "Italian", Description = "Traditional Italian cuisine with pasta, pizza, and more" },
            new() { Name = "Japanese", Description = "Authentic Japanese dishes including sushi, ramen, and tempura" },
            new() { Name = "Mexican", Description = "Vibrant Mexican food with tacos, burritos, and enchiladas" },
        };

        await collection.InsertManyAsync(categories);
    }

    private static async Task<(ObjectId, ObjectId)> SeedRestaurantsAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Restaurant>(CollectionNames.Restaurants);
        if (await collection.CountDocumentsAsync(FilterDefinition<Restaurant>.Empty) > 0)
        {
            var existing = await collection.Find(FilterDefinition<Restaurant>.Empty)
                .Limit(2)
                .ToListAsync();
            return (existing[0].Id, existing.Count > 1 ? existing[1].Id : existing[0].Id);
        }

        var restaurant1 = new Restaurant
        {
            Name = "Bella Napoli",
            Address = "123 Main Street, Downtown",
            Phone = "+1-555-0101",
        };

        var restaurant2 = new Restaurant
        {
            Name = "Tokyo Garden",
            Address = "456 Oak Avenue, Midtown",
            Phone = "+1-555-0202",
        };

        await collection.InsertManyAsync(new[] { restaurant1, restaurant2 });
        return (restaurant1.Id, restaurant2.Id);
    }

    private static async Task SeedRestaurantProfilesAsync(
        IMongoDatabase database, ObjectId restaurant1Id, ObjectId restaurant2Id)
    {
        var collection = database.GetCollection<RestaurantProfile>(CollectionNames.RestaurantProfiles);
        if (await collection.CountDocumentsAsync(FilterDefinition<RestaurantProfile>.Empty) > 0)
            return;

        var profiles = new List<RestaurantProfile>
        {
            new()
            {
                RestaurantId = restaurant1Id,
                Website = "https://bellanapoli.example.com",
                OpeningHours = "Mon-Sun 11:00-23:00",
                Capacity = 80,
            },
            new()
            {
                RestaurantId = restaurant2Id,
                Website = "https://tokyogarden.example.com",
                OpeningHours = "Tue-Sun 12:00-22:00",
                Capacity = 50,
            },
        };

        await collection.InsertManyAsync(profiles);
    }

    private static async Task<List<ObjectId>> SeedIngredientsAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Ingredient>(CollectionNames.Ingredients);
        if (await collection.CountDocumentsAsync(FilterDefinition<Ingredient>.Empty) > 0)
        {
            var existing = await collection.Find(FilterDefinition<Ingredient>.Empty).ToListAsync();
            return existing.Select(i => i.Id).ToList();
        }

        var ingredients = new List<Ingredient>
        {
            new() { Name = "Mozzarella", IsAllergen = true },
            new() { Name = "Tomato Sauce", IsAllergen = false },
            new() { Name = "Fresh Salmon", IsAllergen = true },
            new() { Name = "Soy Sauce", IsAllergen = true },
            new() { Name = "Basil", IsAllergen = false },
        };

        await collection.InsertManyAsync(ingredients);
        return ingredients.Select(i => i.Id).ToList();
    }

    private static async Task SeedMenuItemsAsync(
        IMongoDatabase database,
        ObjectId restaurant1Id,
        ObjectId restaurant2Id,
        List<ObjectId> ingredientIds)
    {
        var collection = database.GetCollection<MenuItem>(CollectionNames.MenuItems);
        if (await collection.CountDocumentsAsync(FilterDefinition<MenuItem>.Empty) > 0)
            return;

        var menuItems = new List<MenuItem>
        {
            new()
            {
                RestaurantId = restaurant1Id,
                Name = "Margherita Pizza",
                Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                Price = 14.99m,
                IsAvailable = true,
                IngredientIds = new List<ObjectId> { ingredientIds[0], ingredientIds[1], ingredientIds[4] },
            },
            new()
            {
                RestaurantId = restaurant1Id,
                Name = "Spaghetti Carbonara",
                Description = "Traditional Roman pasta with egg, cheese, and pancetta",
                Price = 16.50m,
                IsAvailable = true,
                IngredientIds = new List<ObjectId> { ingredientIds[0] },
            },
            new()
            {
                RestaurantId = restaurant2Id,
                Name = "Salmon Sashimi",
                Description = "Fresh sliced salmon served with soy sauce and wasabi",
                Price = 18.00m,
                IsAvailable = true,
                IngredientIds = new List<ObjectId> { ingredientIds[2], ingredientIds[3] },
            },
            new()
            {
                RestaurantId = restaurant2Id,
                Name = "Miso Ramen",
                Description = "Rich miso broth with noodles, pork, and soft-boiled egg",
                Price = 15.00m,
                IsAvailable = true,
                IngredientIds = new List<ObjectId> { ingredientIds[3] },
            },
        };

        await collection.InsertManyAsync(menuItems);
    }

    private static async Task SeedGiftsAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Gift>(CollectionNames.Gifts);
        if (await collection.CountDocumentsAsync(FilterDefinition<Gift>.Empty) > 0)
            return;

        var gifts = new List<Gift>
        {
            new()
            {
                Name = "Gourmet Chocolate Box",
                IsWrapped = true,
                TrackingCode = Guid.NewGuid(),
                Price = 29.99m,
                Barcode = 5901234123457,
                Weight = 0.5,
                Rating = 4.8f,
                QuantityInStock = 150,
                MinAge = 3,
                ShippedAt = DateTimeOffset.UtcNow.AddDays(-2),
                Description = "Premium assorted chocolates in an elegant gift box",
                Notes = "Store in a cool, dry place",
            },
            new()
            {
                Name = "Ceramic Tea Set",
                IsWrapped = false,
                TrackingCode = Guid.NewGuid(),
                Price = 45.00m,
                Barcode = 5901234123464,
                Weight = 1.2,
                Rating = 4.5f,
                QuantityInStock = 40,
                MinAge = 12,
                ShippedAt = DateTimeOffset.UtcNow.AddDays(-5),
                Description = "Hand-painted ceramic tea set with teapot and four cups",
                Notes = "Fragile - handle with care",
            },
        };

        await collection.InsertManyAsync(gifts);
    }
}
