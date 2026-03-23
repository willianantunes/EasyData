using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage;
using NDjango.Admin.MongoDB.Authentication.Entities;

namespace NDjango.Admin.MongoDB.Authentication.Storage
{
    internal class MongoAuthStorageQueries : IAdminAuthQueries
    {
        private readonly IMongoDatabase _database;

        public MongoAuthStorageQueries(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<(string Id, string Username, string PasswordHash, bool IsSuperuser, bool IsActive)?> GetUserByUsernameAsync(
            string username, CancellationToken ct = default)
        {
            var collection = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = await collection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync(ct);

            if (user == null)
                return null;

            return (user.Id.ToString(), user.Username, user.Password, user.IsSuperuser, user.IsActive);
        }

        public async Task UpdateLastLoginAsync(string userId, CancellationToken ct = default)
        {
            var objectId = ObjectId.Parse(userId);
            var collection = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            await collection.UpdateOneAsync(
                u => u.Id == objectId,
                Builders<MongoAuthUser>.Update.Set(u => u.LastLogin, DateTime.UtcNow),
                cancellationToken: ct);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
        {
            var objectId = ObjectId.Parse(userId);

            var userGroupsCol = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);
            var userGroups = await userGroupsCol
                .Find(ug => ug.UserId == objectId)
                .ToListAsync(ct);

            var groupIds = userGroups.Select(ug => ug.GroupId).ToList();
            if (groupIds.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var groupPermsCol = _database.GetCollection<MongoAuthGroupPermission>(AuthCollectionNames.GroupPermissions);
            var groupPerms = await groupPermsCol
                .Find(gp => groupIds.Contains(gp.GroupId))
                .ToListAsync(ct);

            var permIds = groupPerms.Select(gp => gp.PermissionId).Distinct().ToList();
            if (permIds.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var permsCol = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            var perms = await permsCol
                .Find(p => permIds.Contains(p.Id))
                .ToListAsync(ct);

            return new HashSet<string>(
                perms.Select(p => p.Codename),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task SeedPermissionsAsync(
            IEnumerable<(string Codename, string Name)> permissions, CancellationToken ct = default)
        {
            var collection = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);

            var existingCodenames = await collection
                .Find(FilterDefinition<MongoAuthPermission>.Empty)
                .Project(p => p.Codename)
                .ToListAsync(ct);
            var existingSet = new HashSet<string>(existingCodenames, StringComparer.OrdinalIgnoreCase);

            var toInsert = permissions
                .Where(p => !existingSet.Contains(p.Codename))
                .Select(p => new MongoAuthPermission { Name = p.Name, Codename = p.Codename })
                .ToList();

            if (toInsert.Count > 0) {
                try {
                    await collection.InsertManyAsync(toInsert,
                        new InsertManyOptions { IsOrdered = false }, ct);
                }
                catch (MongoBulkWriteException ex) when (ex.WriteErrors.All(e => e.Category == ServerErrorCategory.DuplicateKey)) {
                    // Concurrent bootstrap seeded some permissions — safe to ignore
                }
            }
        }

        public async Task CreateDefaultAdminUserAsync(string password, CancellationToken ct = default)
        {
            var collection = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);

            try {
                await collection.InsertOneAsync(new MongoAuthUser
                {
                    Username = "admin",
                    Password = PasswordHasher.HashPassword(password),
                    IsSuperuser = true,
                    IsActive = true,
                    DateJoined = DateTime.UtcNow,
                }, cancellationToken: ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey) {
                // Admin user already exists — safe to ignore
            }
        }

        public async Task<string> CreateOrUpdateSamlUserAsync(string username, CancellationToken ct = default)
        {
            var collection = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var existing = await collection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync(ct);

            if (existing != null) {
                await collection.UpdateOneAsync(
                    u => u.Id == existing.Id,
                    Builders<MongoAuthUser>.Update.Set(u => u.LastLogin, DateTime.UtcNow),
                    cancellationToken: ct);
                return existing.Id.ToString();
            }

            var newUser = new MongoAuthUser
            {
                Username = username,
                Password = PasswordHasher.HashPassword(Guid.NewGuid().ToString("N")),
                IsSuperuser = false,
                IsActive = true,
                DateJoined = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
            };

            try {
                await collection.InsertOneAsync(newUser, cancellationToken: ct);
                return newUser.Id.ToString();
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey) {
                // Concurrent insert won — fetch the existing user
                var inserted = await collection.Find(u => u.Username == username).FirstOrDefaultAsync(ct);
                return inserted.Id.ToString();
            }
        }

        public async Task SyncUserGroupsAsync(string userId, List<string> samlGroupIds, CancellationToken ct = default)
        {
            var objectId = ObjectId.Parse(userId);

            var userGroupsCol = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);

            using var session = await _database.Client.StartSessionAsync(cancellationToken: ct);
            session.StartTransaction();
            try {
                // Remove all existing memberships
                await userGroupsCol.DeleteManyAsync(session, ug => ug.UserId == objectId, cancellationToken: ct);

                if (samlGroupIds != null && samlGroupIds.Count > 0) {
                    // Find matching groups by name
                    var groupsCol = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
                    var trimmedIds = samlGroupIds.Select(g => g.Trim()).ToList();
                    var matchingGroups = await groupsCol
                        .Find(session, g => trimmedIds.Contains(g.Name))
                        .ToListAsync(ct);

                    // Insert new memberships
                    var newMemberships = matchingGroups.Select(g => new MongoAuthUserGroup
                    {
                        UserId = objectId,
                        GroupId = g.Id,
                    }).ToList();

                    if (newMemberships.Count > 0)
                        await userGroupsCol.InsertManyAsync(session, newMemberships, cancellationToken: ct);
                }

                await session.CommitTransactionAsync(ct);
            }
            catch {
                await session.AbortTransactionAsync(ct);
                throw;
            }
        }
    }
}
