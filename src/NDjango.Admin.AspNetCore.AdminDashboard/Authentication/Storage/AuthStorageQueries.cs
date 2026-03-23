using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage
{
    internal class AuthStorageQueries : IAdminAuthQueries
    {
        private readonly AuthDbContext _dbContext;

        public AuthStorageQueries(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(string Id, string Username, string PasswordHash, bool IsSuperuser, bool IsActive)?> GetUserByUsernameAsync(
            string username, CancellationToken ct = default)
        {
            var conn = _dbContext.Database.GetDbConnection();
            await conn.OpenAsync(ct);
            try {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, username, password, is_superuser, is_active FROM dbo.auth_user WHERE username = @username";
                var param = cmd.CreateParameter();
                param.ParameterName = "@username";
                param.Value = username;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct)) {
                    return (
                        reader.GetInt32(0).ToString(),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetBoolean(3),
                        reader.GetBoolean(4)
                    );
                }
                return null;
            }
            finally {
                await conn.CloseAsync();
            }
        }

        public async Task UpdateLastLoginAsync(string userId, CancellationToken ct = default)
        {
            var intUserId = int.Parse(userId);
            await _dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE dbo.auth_user SET last_login = GETUTCDATE() WHERE id = {0}",
                new object[] { intUserId },
                ct);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
        {
            var intUserId = int.Parse(userId);
            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var conn = _dbContext.Database.GetDbConnection();
            await conn.OpenAsync(ct);
            try {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
SELECT DISTINCT p.codename FROM auth_permission p
JOIN auth_group_permissions gp ON gp.permission_id = p.id
JOIN auth_user_groups ug ON ug.group_id = gp.group_id
WHERE ug.user_id = @userId";
                var param = cmd.CreateParameter();
                param.ParameterName = "@userId";
                param.Value = intUserId;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct)) {
                    permissions.Add(reader.GetString(0));
                }
            }
            finally {
                await conn.CloseAsync();
            }
            return permissions;
        }

        public async Task SeedPermissionsAsync(IEnumerable<(string Codename, string Name)> permissions, CancellationToken ct = default)
        {
            foreach (var (codename, name) in permissions) {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    @"IF NOT EXISTS (SELECT 1 FROM auth_permission WHERE codename = {0})
                      INSERT INTO auth_permission (name, codename) VALUES ({1}, {0})",
                    new object[] { codename, name },
                    ct);
            }
        }

        public async Task CreateDefaultAdminUserAsync(string password, CancellationToken ct = default)
        {
            var hashedPassword = PasswordHasher.HashPassword(password);
            await _dbContext.Database.ExecuteSqlRawAsync(
                @"IF NOT EXISTS (SELECT 1 FROM auth_user WHERE username = N'admin')
                  INSERT INTO auth_user (username, password, is_superuser, is_active, date_joined)
                  VALUES (N'admin', {0}, 1, 1, GETUTCDATE())",
                new object[] { hashedPassword },
                ct);
        }

        public async Task<string> CreateOrUpdateSamlUserAsync(string username, CancellationToken ct = default)
        {
            var randomHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString("N"));

            await _dbContext.Database.ExecuteSqlRawAsync(
                @"IF NOT EXISTS (SELECT 1 FROM auth_user WHERE username = {0})
                  INSERT INTO auth_user (username, password, is_superuser, is_active, date_joined, last_login)
                  VALUES ({0}, {1}, 0, 1, GETUTCDATE(), GETUTCDATE())
                ELSE
                  UPDATE auth_user SET last_login = GETUTCDATE() WHERE username = {0}",
                new object[] { username, randomHash },
                ct);

            var conn = _dbContext.Database.GetDbConnection();
            await conn.OpenAsync(ct);
            try {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id FROM dbo.auth_user WHERE username = @username";
                var param = cmd.CreateParameter();
                param.ParameterName = "@username";
                param.Value = username;
                cmd.Parameters.Add(param);

                var result = await cmd.ExecuteScalarAsync(ct);
                return Convert.ToInt32(result).ToString();
            }
            finally {
                await conn.CloseAsync();
            }
        }

        public async Task SyncUserGroupsAsync(string userId, List<string> samlGroupIds, CancellationToken ct = default)
        {
            var intUserId = int.Parse(userId);

            // Remove all existing group memberships
            await _dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM auth_user_groups WHERE user_id = {0}",
                new object[] { intUserId },
                ct);

            if (samlGroupIds == null || samlGroupIds.Count == 0)
                return;

            // Add memberships for matching groups using parameterized query
            var conn = _dbContext.Database.GetDbConnection();
            await conn.OpenAsync(ct);
            try {
                using var cmd = conn.CreateCommand();

                var paramNames = new List<string>();
                for (var i = 0; i < samlGroupIds.Count; i++) {
                    var paramName = $"@g{i}";
                    paramNames.Add(paramName);
                    var param = cmd.CreateParameter();
                    param.ParameterName = paramName;
                    param.Value = samlGroupIds[i].Trim();
                    cmd.Parameters.Add(param);
                }

                var userIdParam = cmd.CreateParameter();
                userIdParam.ParameterName = "@userId";
                userIdParam.Value = intUserId;
                cmd.Parameters.Add(userIdParam);

                cmd.CommandText = $@"INSERT INTO auth_user_groups (user_id, group_id)
                    SELECT @userId, g.id FROM auth_group g WHERE g.name IN ({string.Join(", ", paramNames)})";

                await cmd.ExecuteNonQueryAsync(ct);
            }
            finally {
                await conn.CloseAsync();
            }
        }
    }
}
