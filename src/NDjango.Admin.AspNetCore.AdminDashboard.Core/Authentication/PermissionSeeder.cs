using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage;
using NDjango.Admin.AspNetCore.AdminDashboard.Services;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication
{
    internal class PermissionSeeder
    {
        private readonly IAdminAuthQueries _queries;

        public PermissionSeeder(IAdminAuthQueries queries)
        {
            _queries = queries;
        }

        public async Task SeedPermissionsAsync(MetaData model, CancellationToken ct = default)
        {
            var permissions = new List<(string Codename, string Name)>();

            foreach (var entity in model.EntityRoot.SubEntities) {
                var entityName = AdminMetadataService.GetEntityName(entity);
                var entityNameLower = entityName.ToLowerInvariant();
                var displayName = entity.Name;

                permissions.Add(($"add_{entityNameLower}", $"Can add {displayName}"));
                permissions.Add(($"change_{entityNameLower}", $"Can change {displayName}"));
                permissions.Add(($"delete_{entityNameLower}", $"Can delete {displayName}"));
                permissions.Add(($"view_{entityNameLower}", $"Can view {displayName}"));
            }

            await _queries.SeedPermissionsAsync(permissions, ct);
        }
    }
}
