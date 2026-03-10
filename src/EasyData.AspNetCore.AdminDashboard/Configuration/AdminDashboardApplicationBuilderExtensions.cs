using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using EasyData.AspNetCore.AdminDashboard;
using EasyData.AspNetCore.AdminDashboard.Authentication;
using EasyData.AspNetCore.AdminDashboard.Authentication.Storage;
using EasyData.Services;

namespace Microsoft.AspNetCore.Builder
{
    public static class AdminDashboardApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEasyDataAdminDashboard(
            this IApplicationBuilder app,
            string path = "/admin",
            AdminDashboardOptions options = null)
        {
            options ??= new AdminDashboardOptions();
            path = "/" + path.Trim('/');

            var easyDataOptions = (EasyDataOptions)app.ApplicationServices.GetService(typeof(EasyDataOptions));
            if (easyDataOptions == null)
            {
                throw new InvalidOperationException(
                    "EasyDataOptions is not registered. " +
                    "Call services.AddEasyDataAdminDashboard<TDbContext>() in ConfigureServices first.");
            }

            if (options.RequireAuthentication)
            {
                BootstrapAuthentication(app, options, easyDataOptions);
            }

            app.UseMiddleware<AdminDashboardMiddleware>(options, easyDataOptions, path);

            return app;
        }

        private static void BootstrapAuthentication(IApplicationBuilder app, AdminDashboardOptions options, EasyDataOptions easyDataOptions)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Initialize auth tables
            var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var storageInitializer = new SqlServerAuthStorageInitializer(authDbContext);
            storageInitializer.InitializeAsync().GetAwaiter().GetResult();

            // Store the original resolver and wrap with composite
            var originalResolver = easyDataOptions.ManagerResolver;
            var authEasyDataOptions = new EasyDataOptions();
            authEasyDataOptions.UseDbContext<AuthDbContext>();

            var dbContextType = AdminDashboardServiceCollectionExtensions.DbContextType;

            easyDataOptions.UseManager((services, opts) =>
            {
                var userManager = originalResolver(services, opts);
                var authManager = authEasyDataOptions.ManagerResolver(services, authEasyDataOptions);
                var userDbContext = (DbContext)services.GetService(dbContextType);
                var authDbCtx = services.GetService(typeof(AuthDbContext)) as DbContext;
                return new CompositeEasyDataManager(services, opts, userManager, authManager, userDbContext, authDbCtx);
            });

            // Seed permissions
            using var seedScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var seedManager = easyDataOptions.ManagerResolver(seedScope.ServiceProvider, easyDataOptions);
            var model = seedManager.GetModelAsync("__admin").GetAwaiter().GetResult();

            using var seedAuthDbContext = new AuthDbContext(GetAuthDbContextOptions(seedScope.ServiceProvider));
            var queries = new AuthStorageQueries(seedAuthDbContext);
            var seeder = new PermissionSeeder(queries);
            seeder.SeedPermissionsAsync(model).GetAwaiter().GetResult();

            // Create default admin user
            if (options.CreateDefaultAdminUser)
            {
                queries.CreateDefaultAdminUserAsync(options.DefaultAdminPassword).GetAwaiter().GetResult();
            }
        }

        private static DbContextOptions<AuthDbContext> GetAuthDbContextOptions(IServiceProvider serviceProvider)
        {
            var dbContextType = AdminDashboardServiceCollectionExtensions.DbContextType;
            var userDbContext = (DbContext)serviceProvider.GetService(dbContextType);
            var connectionString = userDbContext.Database.GetConnectionString();

            return new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(connectionString)
                .Options;
        }
    }
}
