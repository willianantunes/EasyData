using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Xunit;

using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.AuthenticationTests
{
    public class StorageInitializerTests : IDisposable
    {
        private readonly string _dbName;
        private readonly DbContextOptions<AuthDbContext> _options;

        private const string ConnectionStringTemplate =
            "Server=localhost,1433;Database={0};User Id=sa;Password=Password1;TrustServerCertificate=true;";

        public StorageInitializerTests()
        {
            _dbName = $"NDjangoAdminStorageTest_{Guid.NewGuid():N}";
            var connectionString = string.Format(ConnectionStringTemplate, _dbName);

            var masterOptions = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(string.Format(ConnectionStringTemplate, "master"))
                .Options;
            using (var ctx = new AuthDbContext(masterOptions))
            {
                ctx.Database.ExecuteSqlRaw($"CREATE DATABASE [{_dbName}]");
            }

            _options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(connectionString)
                .Options;
        }

        [Fact]
        public async Task InitializeAsync_NewDatabase_CreatesAllTablesAsync()
        {
            // Arrange
            using var dbContext = new AuthDbContext(_options);
            var initializer = new SqlServerAuthStorageInitializer(dbContext);

            // Act
            await initializer.InitializeAsync();

            // Assert
            var conn = dbContext.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN
                ('auth_user', 'auth_group', 'auth_permission', 'auth_group_permissions', 'auth_user_groups')";
            var count = (int)await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();

            Assert.Equal(5, count);
        }

        [Fact]
        public async Task InitializeAsync_CalledTwice_DoesNotThrowAsync()
        {
            // Arrange
            using var dbContext = new AuthDbContext(_options);
            var initializer = new SqlServerAuthStorageInitializer(dbContext);

            // Act
            await initializer.InitializeAsync();
            var exception = await Record.ExceptionAsync(() => initializer.InitializeAsync());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task CreateDefaultAdminUserAsync_NewDatabase_CreatesAdminUserAsync()
        {
            // Arrange
            using var dbContext = new AuthDbContext(_options);
            var initializer = new SqlServerAuthStorageInitializer(dbContext);
            await initializer.InitializeAsync();
            var queries = new AuthStorageQueries(dbContext);

            // Act
            await queries.CreateDefaultAdminUserAsync("admin");

            // Assert
            var user = await queries.GetUserByUsernameAsync("admin");
            Assert.NotNull(user);
            Assert.Equal("admin", user.Value.Username);
            Assert.True(user.Value.IsSuperuser);
        }

        [Fact]
        public async Task CreateDefaultAdminUserAsync_CalledTwice_DoesNotThrowAsync()
        {
            // Arrange
            using var dbContext = new AuthDbContext(_options);
            var initializer = new SqlServerAuthStorageInitializer(dbContext);
            await initializer.InitializeAsync();
            var queries = new AuthStorageQueries(dbContext);

            // Act
            await queries.CreateDefaultAdminUserAsync("admin");
            var exception = await Record.ExceptionAsync(() => queries.CreateDefaultAdminUserAsync("admin"));

            // Assert
            Assert.Null(exception);
        }

        public void Dispose()
        {
            try
            {
                var masterOptions = new DbContextOptionsBuilder<AuthDbContext>()
                    .UseSqlServer(string.Format(ConnectionStringTemplate, "master"))
                    .Options;
                using var ctx = new AuthDbContext(masterOptions);
                ctx.Database.ExecuteSqlRaw(
                    $"IF DB_ID('{_dbName}') IS NOT NULL BEGIN ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_dbName}]; END");
            }
            catch { }
        }
    }
}
