using System;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

using NDjango.Admin.AspNetCore.AdminDashboard;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using NDjango.Admin.EntityFrameworkCore;
using NDjango.Admin.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdminDashboardServiceCollectionExtensions
    {
        private static Type _dbContextType;

        internal static Type DbContextType => _dbContextType;

        public static IServiceCollection AddNDjangoAdminDashboard<TDbContext>(
            this IServiceCollection services,
            Action<DbContextMetaDataLoaderOptions> loaderOptionsBuilder = null)
            where TDbContext : DbContext
        {
            _dbContextType = typeof(TDbContext);

            services.AddSingleton(sp => {
                var ndjangoAdminOptions = new NDjangoAdminOptions();
                ndjangoAdminOptions.UseDbContext<TDbContext>(loaderOptionsBuilder);
                return ndjangoAdminOptions;
            });

            // Register auth services eagerly (they are no-ops if RequireAuthentication is false)
            RegisterAuthServices<TDbContext>(services);

            return services;
        }

        private static void RegisterAuthServices<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddDataProtection();

            services.AddDbContext<AuthDbContext>((sp, options) => {
                var userDbContext = sp.GetRequiredService<TDbContext>();
                var connectionString = userDbContext.Database.GetConnectionString();
                options.UseSqlServer(connectionString);
            });

            // AdminCookieAuthService is created per-request in the middleware
            // because AdminDashboardOptions is not in DI
        }
    }
}
