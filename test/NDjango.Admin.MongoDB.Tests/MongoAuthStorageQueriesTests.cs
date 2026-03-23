using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using NDjango.Admin.MongoDB.Authentication.Entities;
using NDjango.Admin.MongoDB.Authentication.Storage;
using Xunit;

namespace NDjango.Admin.MongoDB.Tests
{
    public class MongoAuthStorageQueriesTests : IDisposable
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly string _dbName;
        private readonly MongoAuthStorageQueries _queries;

        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017/?directConnection=true";

        public MongoAuthStorageQueriesTests()
        {
            _dbName = $"MongoAuthQueriesTest_{Guid.NewGuid():N}";
            _mongoClient = new MongoClient(ConnectionString);
            _database = _mongoClient.GetDatabase(_dbName);
            _queries = new MongoAuthStorageQueries(_database);

            var initializer = new MongoAuthStorageInitializer(_database);
            initializer.InitializeAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUserDataAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser
            {
                Username = "testadmin",
                Password = PasswordHasher.HashPassword("secret"),
                IsSuperuser = true,
                IsActive = true,
            };
            await usersCol.InsertOneAsync(user);

            // Act
            var result = await _queries.GetUserByUsernameAsync("testadmin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id.ToString(), result.Value.Id);
            Assert.Equal("testadmin", result.Value.Username);
            Assert.True(result.Value.IsSuperuser);
            Assert.True(result.Value.IsActive);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_NonExistentUser_ReturnsNullAsync()
        {
            // Arrange
            // No users inserted

            // Act
            var result = await _queries.GetUserByUsernameAsync("nobody");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateLastLoginAsync_ExistingUser_UpdatesLastLoginFieldAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser
            {
                Username = "updatetest",
                Password = "hash",
                IsSuperuser = false,
                IsActive = true,
            };
            await usersCol.InsertOneAsync(user);

            // Act
            await _queries.UpdateLastLoginAsync(user.Id.ToString());

            // Assert
            var updated = await usersCol.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
            Assert.NotNull(updated.LastLogin);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_UserWithGroupPermissions_ReturnsCodeNamesAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser { Username = "permuser", Password = "h", IsActive = true };
            await usersCol.InsertOneAsync(user);

            var groupsCol = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var group = new MongoAuthGroup { Name = "TestGroup" };
            await groupsCol.InsertOneAsync(group);

            var permsCol = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            var perm1 = new MongoAuthPermission { Name = "Can view cat", Codename = "view_category" };
            var perm2 = new MongoAuthPermission { Name = "Can add cat", Codename = "add_category" };
            await permsCol.InsertManyAsync(new[] { perm1, perm2 });

            var gpCol = _database.GetCollection<MongoAuthGroupPermission>(AuthCollectionNames.GroupPermissions);
            await gpCol.InsertManyAsync(new[]
            {
                new MongoAuthGroupPermission { GroupId = group.Id, PermissionId = perm1.Id },
                new MongoAuthGroupPermission { GroupId = group.Id, PermissionId = perm2.Id },
            });

            var ugCol = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);
            await ugCol.InsertOneAsync(new MongoAuthUserGroup { UserId = user.Id, GroupId = group.Id });

            // Act
            var permissions = await _queries.GetUserPermissionsAsync(user.Id.ToString());

            // Assert
            Assert.Contains("view_category", permissions);
            Assert.Contains("add_category", permissions);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_UserWithNoGroups_ReturnsEmptySetAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser { Username = "lonely", Password = "h", IsActive = true };
            await usersCol.InsertOneAsync(user);

            // Act
            var permissions = await _queries.GetUserPermissionsAsync(user.Id.ToString());

            // Assert
            Assert.Empty(permissions);
        }

        [Fact]
        public async Task SeedPermissionsAsync_NewPermissions_InsertsThemAsync()
        {
            // Arrange
            var perms = new List<(string, string)>
            {
                ("view_item", "Can view item"),
                ("add_item", "Can add item"),
            };

            // Act
            await _queries.SeedPermissionsAsync(perms);

            // Assert
            var permsCol = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            var count = await permsCol.CountDocumentsAsync(Builders<MongoAuthPermission>.Filter.Empty);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task SeedPermissionsAsync_DuplicatePermissions_DoesNotInsertDuplicatesAsync()
        {
            // Arrange
            var perms = new List<(string, string)>
            {
                ("view_item", "Can view item"),
            };
            await _queries.SeedPermissionsAsync(perms);

            // Act — seed again
            await _queries.SeedPermissionsAsync(perms);

            // Assert
            var permsCol = _database.GetCollection<MongoAuthPermission>(AuthCollectionNames.Permissions);
            var count = await permsCol.CountDocumentsAsync(p => p.Codename == "view_item");
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CreateDefaultAdminUserAsync_NoExistingAdmin_CreatesAdminAsync()
        {
            // Arrange
            // No users exist

            // Act
            await _queries.CreateDefaultAdminUserAsync("secretpass");

            // Assert
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var admin = await usersCol.Find(u => u.Username == "admin").FirstOrDefaultAsync();
            Assert.NotNull(admin);
            Assert.True(admin.IsSuperuser);
            Assert.True(admin.IsActive);
            Assert.True(PasswordHasher.VerifyPassword("secretpass", admin.Password));
        }

        [Fact]
        public async Task CreateDefaultAdminUserAsync_ExistingAdmin_DoesNotDuplicateAsync()
        {
            // Arrange
            await _queries.CreateDefaultAdminUserAsync("pass1");

            // Act
            await _queries.CreateDefaultAdminUserAsync("pass2");

            // Assert
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var count = await usersCol.CountDocumentsAsync(u => u.Username == "admin");
            Assert.Equal(1, count);
            // Original password should be kept
            var admin = await usersCol.Find(u => u.Username == "admin").FirstOrDefaultAsync();
            Assert.True(PasswordHasher.VerifyPassword("pass1", admin.Password));
        }

        [Fact]
        public async Task CreateOrUpdateSamlUserAsync_NewUser_CreatesUserAndReturnsIdAsync()
        {
            // Arrange
            // No users

            // Act
            var userId = await _queries.CreateOrUpdateSamlUserAsync("samluser@company.com");

            // Assert
            Assert.False(string.IsNullOrEmpty(userId));
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = await usersCol.Find(u => u.Username == "samluser@company.com").FirstOrDefaultAsync();
            Assert.NotNull(user);
            Assert.False(user.IsSuperuser);
            Assert.True(user.IsActive);
            Assert.Equal(user.Id.ToString(), userId);
        }

        [Fact]
        public async Task CreateOrUpdateSamlUserAsync_ExistingUser_UpdatesLastLoginAsync()
        {
            // Arrange
            var userId1 = await _queries.CreateOrUpdateSamlUserAsync("saml_existing@co.com");

            // Act
            var userId2 = await _queries.CreateOrUpdateSamlUserAsync("saml_existing@co.com");

            // Assert
            Assert.Equal(userId1, userId2);
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var count = await usersCol.CountDocumentsAsync(u => u.Username == "saml_existing@co.com");
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task SyncUserGroupsAsync_AssignsMatchingGroupsAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser { Username = "syncuser", Password = "h", IsActive = true };
            await usersCol.InsertOneAsync(user);

            var groupsCol = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var group1 = new MongoAuthGroup { Name = "Group-A" };
            var group2 = new MongoAuthGroup { Name = "Group-B" };
            await groupsCol.InsertManyAsync(new[] { group1, group2 });

            // Act
            await _queries.SyncUserGroupsAsync(user.Id.ToString(), new List<string> { "Group-A", "Group-B" });

            // Assert
            var ugCol = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);
            var memberships = await ugCol.Find(ug => ug.UserId == user.Id).ToListAsync();
            Assert.Equal(2, memberships.Count);
        }

        [Fact]
        public async Task SyncUserGroupsAsync_ReplacesExistingMembershipsAsync()
        {
            // Arrange
            var usersCol = _database.GetCollection<MongoAuthUser>(AuthCollectionNames.Users);
            var user = new MongoAuthUser { Username = "resyncuser", Password = "h", IsActive = true };
            await usersCol.InsertOneAsync(user);

            var groupsCol = _database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var group1 = new MongoAuthGroup { Name = "Old-Group" };
            var group2 = new MongoAuthGroup { Name = "New-Group" };
            await groupsCol.InsertManyAsync(new[] { group1, group2 });

            // Assign to Old-Group first
            await _queries.SyncUserGroupsAsync(user.Id.ToString(), new List<string> { "Old-Group" });

            // Act — re-sync with only New-Group
            await _queries.SyncUserGroupsAsync(user.Id.ToString(), new List<string> { "New-Group" });

            // Assert
            var ugCol = _database.GetCollection<MongoAuthUserGroup>(AuthCollectionNames.UserGroups);
            var memberships = await ugCol.Find(ug => ug.UserId == user.Id).ToListAsync();
            Assert.Single(memberships);
            Assert.Equal(group2.Id, memberships[0].GroupId);
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
