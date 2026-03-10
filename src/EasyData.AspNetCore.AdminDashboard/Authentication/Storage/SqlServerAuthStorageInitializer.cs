using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EasyData.AspNetCore.AdminDashboard.Authentication.Storage
{
    internal class SqlServerAuthStorageInitializer : IAuthStorageInitializer
    {
        private readonly AuthDbContext _dbContext;

        public SqlServerAuthStorageInitializer(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(CreateAuthUserTable, ct);
            await _dbContext.Database.ExecuteSqlRawAsync(CreateAuthGroupTable, ct);
            await _dbContext.Database.ExecuteSqlRawAsync(CreateAuthPermissionTable, ct);
            await _dbContext.Database.ExecuteSqlRawAsync(CreateAuthGroupPermissionsTable, ct);
            await _dbContext.Database.ExecuteSqlRawAsync(CreateAuthUserGroupsTable, ct);
        }

        private const string CreateAuthUserTable = @"
IF OBJECT_ID(N'dbo.auth_user', N'U') IS NULL
CREATE TABLE dbo.auth_user (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(150) NOT NULL,
    password NVARCHAR(256) NOT NULL,
    last_login DATETIME2 NULL,
    is_superuser BIT NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,
    date_joined DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_auth_user_username UNIQUE (username)
);";

        private const string CreateAuthGroupTable = @"
IF OBJECT_ID(N'dbo.auth_group', N'U') IS NULL
CREATE TABLE dbo.auth_group (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(150) NOT NULL,
    CONSTRAINT UQ_auth_group_name UNIQUE (name)
);";

        private const string CreateAuthPermissionTable = @"
IF OBJECT_ID(N'dbo.auth_permission', N'U') IS NULL
CREATE TABLE dbo.auth_permission (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    codename NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_auth_permission_codename UNIQUE (codename)
);";

        private const string CreateAuthGroupPermissionsTable = @"
IF OBJECT_ID(N'dbo.auth_group_permissions', N'U') IS NULL
CREATE TABLE dbo.auth_group_permissions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    group_id INT NOT NULL,
    permission_id INT NOT NULL,
    CONSTRAINT FK_auth_group_permissions_group FOREIGN KEY (group_id) REFERENCES dbo.auth_group(id),
    CONSTRAINT FK_auth_group_permissions_permission FOREIGN KEY (permission_id) REFERENCES dbo.auth_permission(id),
    CONSTRAINT UQ_auth_group_permissions UNIQUE (group_id, permission_id)
);";

        private const string CreateAuthUserGroupsTable = @"
IF OBJECT_ID(N'dbo.auth_user_groups', N'U') IS NULL
CREATE TABLE dbo.auth_user_groups (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    group_id INT NOT NULL,
    CONSTRAINT FK_auth_user_groups_user FOREIGN KEY (user_id) REFERENCES dbo.auth_user(id),
    CONSTRAINT FK_auth_user_groups_group FOREIGN KEY (group_id) REFERENCES dbo.auth_group(id),
    CONSTRAINT UQ_auth_user_groups UNIQUE (user_id, group_id)
);";
    }
}
