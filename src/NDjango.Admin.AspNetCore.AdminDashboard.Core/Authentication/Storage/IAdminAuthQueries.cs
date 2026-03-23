using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage
{
    public interface IAdminAuthQueries
    {
        public Task<(string Id, string Username, string PasswordHash, bool IsSuperuser, bool IsActive)?> GetUserByUsernameAsync(
            string username, CancellationToken ct = default);

        public Task UpdateLastLoginAsync(string userId, CancellationToken ct = default);

        public Task<HashSet<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

        public Task SeedPermissionsAsync(IEnumerable<(string Codename, string Name)> permissions, CancellationToken ct = default);

        public Task CreateDefaultAdminUserAsync(string password, CancellationToken ct = default);

        public Task<string> CreateOrUpdateSamlUserAsync(string username, CancellationToken ct = default);

        public Task SyncUserGroupsAsync(string userId, List<string> samlGroupIds, CancellationToken ct = default);
    }
}
