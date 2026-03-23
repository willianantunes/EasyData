using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage
{
    public interface IAdminAuthQueries
    {
        Task<(string Id, string Username, string PasswordHash, bool IsSuperuser, bool IsActive)?> GetUserByUsernameAsync(
            string username, CancellationToken ct = default);

        Task UpdateLastLoginAsync(string userId, CancellationToken ct = default);

        Task<HashSet<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

        Task SeedPermissionsAsync(IEnumerable<(string Codename, string Name)> permissions, CancellationToken ct = default);

        Task CreateDefaultAdminUserAsync(string password, CancellationToken ct = default);

        Task<string> CreateOrUpdateSamlUserAsync(string username, CancellationToken ct = default);

        Task SyncUserGroupsAsync(string userId, List<string> samlGroupIds, CancellationToken ct = default);
    }
}
