using System;

using Microsoft.Extensions.DependencyInjection;

using NDjango.Admin.AspNetCore.AdminDashboard;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using NDjango.Admin.AspNetCore.AdminDashboard.Services;
using NDjango.Admin.Services;

namespace NDjango.Admin.MongoDB
{
    public static class AdminDashboardMongoExtensions
    {
        /// <summary>
        /// Registers the NDjango.Admin dashboard with MongoDB as the data provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="dashboardOptions">Dashboard configuration options.</param>
        /// <param name="mongoOptionsBuilder">Action to configure MongoDB collections.</param>
        public static IServiceCollection AddNDjangoAdminDashboardMongo(
            this IServiceCollection services,
            AdminDashboardOptions dashboardOptions,
            Action<MongoDbOptions> mongoOptionsBuilder)
        {
            dashboardOptions ??= new AdminDashboardOptions();

            services.AddSingleton(dashboardOptions);

            services.AddSingleton(sp => {
                var ndjangoAdminOptions = new NDjangoAdminOptions();
                ndjangoAdminOptions.UseMongoDB(mongoOptionsBuilder);
                return ndjangoAdminOptions;
            });

            services.AddSingleton<ISearchFilterFactory, MongoSearchFilterFactory>();
            services.AddSingleton<AuthBootstrapReadinessState>();

            return services;
        }
    }
}
