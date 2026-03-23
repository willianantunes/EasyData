using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication
{
    internal class PermissionChecker
    {
        private const string PermissionsCacheKey = "NDjango.Admin.Auth.Permissions";
        private readonly IAdminAuthQueries _queries;

        public PermissionChecker(IAdminAuthQueries queries)
        {
            _queries = queries;
        }

        public async Task<bool> HasPermissionAsync(HttpContext httpContext, int userId, bool isSuperuser, string permissionCodename, CancellationToken ct = default)
        {
            if (isSuperuser)
                return true;

            var permissions = await GetCachedPermissionsAsync(httpContext, userId, ct);
            return permissions.Contains(permissionCodename);
        }

        private async Task<HashSet<string>> GetCachedPermissionsAsync(HttpContext httpContext, int userId, CancellationToken ct)
        {
            if (httpContext.Items.TryGetValue(PermissionsCacheKey, out var cached) && cached is HashSet<string> cachedPerms)
                return cachedPerms;

            var permissions = await _queries.GetUserPermissionsAsync(userId, ct);
            httpContext.Items[PermissionsCacheKey] = permissions;
            return permissions;
        }
    }
}
