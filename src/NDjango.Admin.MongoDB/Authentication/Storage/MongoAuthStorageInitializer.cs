using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;

using NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage;
using NDjango.Admin.MongoDB.Authentication.Entities;

namespace NDjango.Admin.MongoDB.Authentication.Storage
{
    internal class MongoAuthStorageInitializer : IAuthStorageInitializer
    {
        private readonly IMongoDatabase _database;

        public MongoAuthStorageInitializer(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var users = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            await users.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAuthUser>(
                    Builders<MongoAuthUser>.IndexKeys.Ascending(u => u.Username),
                    new CreateIndexOptions { Unique = true }),
                cancellationToken: ct);

            var groups = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            await groups.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAuthGroup>(
                    Builders<MongoAuthGroup>.IndexKeys.Ascending(g => g.Name),
                    new CreateIndexOptions { Unique = true }),
                cancellationToken: ct);

            var permissions = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            await permissions.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAuthPermission>(
                    Builders<MongoAuthPermission>.IndexKeys.Ascending(p => p.Codename),
                    new CreateIndexOptions { Unique = true }),
                cancellationToken: ct);

            var groupPermissions = _database.GetCollection<MongoAuthGroupPermission>(AuthCollectionNames.GroupPermissions);
            await groupPermissions.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAuthGroupPermission>(
                    Builders<MongoAuthGroupPermission>.IndexKeys
                        .Ascending(gp => gp.GroupId)
                        .Ascending(gp => gp.PermissionId),
                    new CreateIndexOptions { Unique = true }),
                cancellationToken: ct);

            var userGroups = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);
            await userGroups.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAuthUserGroup>(
                    Builders<MongoAuthUserGroup>.IndexKeys
                        .Ascending(ug => ug.UserId)
                        .Ascending(ug => ug.GroupId),
                    new CreateIndexOptions { Unique = true }),
                cancellationToken: ct);
        }
    }
}
