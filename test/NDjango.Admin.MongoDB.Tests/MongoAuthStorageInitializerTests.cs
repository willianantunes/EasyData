using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NDjango.Admin.MongoDB.Authentication.Entities;
using NDjango.Admin.MongoDB.Authentication.Storage;
using Xunit;

namespace NDjango.Admin.MongoDB.Tests
{
    public class MongoAuthStorageInitializerTests : IDisposable
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly string _dbName;

        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017/?directConnection=true";

        public MongoAuthStorageInitializerTests()
        {
            _dbName = $"MongoAuthInitTest_{Guid.NewGuid():N}";
            _mongoClient = new MongoClient(ConnectionString);
            _database = _mongoClient.GetDatabase(_dbName);
        }

        [Fact]
        public async Task InitializeAsync_CreatesUniqueIndexOnUsersUsernameAsync()
        {
            // Arrange
            var initializer = new MongoAuthStorageInitializer(_database);

            // Act
            await initializer.InitializeAsync();

            // Assert
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var indexes = await (await usersCol.Indexes.ListAsync()).ToListAsync();
            var usernameIndex = indexes.FirstOrDefault(idx =>
            {
                var keys = idx["key"].AsBsonDocument;
                return keys.Contains("Username");
            });
            Assert.NotNull(usernameIndex);
            Assert.True(usernameIndex["unique"].AsBoolean);
        }

        [Fact]
        public async Task InitializeAsync_CreatesUniqueIndexOnGroupsNameAsync()
        {
            // Arrange
            var initializer = new MongoAuthStorageInitializer(_database);

            // Act
            await initializer.InitializeAsync();

            // Assert
            var groupsCol = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var indexes = await (await groupsCol.Indexes.ListAsync()).ToListAsync();
            var nameIndex = indexes.FirstOrDefault(idx =>
            {
                var keys = idx["key"].AsBsonDocument;
                return keys.Contains("Name");
            });
            Assert.NotNull(nameIndex);
            Assert.True(nameIndex["unique"].AsBoolean);
        }

        [Fact]
        public async Task InitializeAsync_CreatesUniqueIndexOnPermissionsCodenameAsync()
        {
            // Arrange
            var initializer = new MongoAuthStorageInitializer(_database);

            // Act
            await initializer.InitializeAsync();

            // Assert
            var permsCol = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            var indexes = await (await permsCol.Indexes.ListAsync()).ToListAsync();
            var codenameIndex = indexes.FirstOrDefault(idx =>
            {
                var keys = idx["key"].AsBsonDocument;
                return keys.Contains("Codename");
            });
            Assert.NotNull(codenameIndex);
            Assert.True(codenameIndex["unique"].AsBoolean);
        }

        [Fact]
        public async Task InitializeAsync_IsIdempotent_DoesNotFailOnSecondCallAsync()
        {
            // Arrange
            var initializer = new MongoAuthStorageInitializer(_database);
            await initializer.InitializeAsync();

            // Act & Assert — should not throw
            await initializer.InitializeAsync();
        }

        public void Dispose()
        {
            try
            {
                _mongoClient.DropDatabase(_dbName);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
