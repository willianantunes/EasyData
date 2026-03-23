using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage
{
    public interface IAdminAuthQueries
    {
        Task<(int Id, string Username, string PasswordHash, bool IsSuperuser, bool IsActive)?> GetUserByUsernameAsync(
            string username, CancellationToken ct = default);

        Task UpdateLastLoginAsync(int userId, CancellationToken ct = default);

        Task<HashSet<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);

        Task SeedPermissionsAsync(IEnumerable<(string Codename, string Name)> permissions, CancellationToken ct = default);

        Task CreateDefaultAdminUserAsync(string password, CancellationToken ct = default);

        Task<int> CreateOrUpdateSamlUserAsync(string username, CancellationToken ct = default);

        Task SyncUserGroupsAsync(int userId, List<string> samlGroupIds, CancellationToken ct = default);
    }
}
