using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NDjango.Admin.AspNetCore.AdminDashboard.Authorization;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.Fixtures
{
    public class AuthEnabledFixture : IDisposable
    {
        private readonly IHost _host;
        private readonly string _dbName;

        private static string ConnectionStringTemplate =
            TestConnectionHelper.ConnectionStringTemplate;

        public AuthEnabledFixture()
        {
            _dbName = $"NDjangoAdminAuthTest_{Guid.NewGuid():N}";
            var connectionString = string.Format(ConnectionStringTemplate, _dbName);

            _host = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddDbContext<TestDbContext>(options =>
                            {
                                options.UseSqlServer(connectionString);
                            });

                            services.AddNDjangoAdminDashboard<TestDbContext>();
                        })
                        .Configure(app =>
                        {
                            // Ensure user DB exists
                            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                            using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                            context.Database.EnsureCreated();

                            // Seed restaurant data
                            var cat1 = new Category { Name = "Italian", Description = "Italian cuisine" };
                            context.Categories.Add(cat1);
                            context.SaveChanges();

                            app.UseNDjangoAdminDashboard("/admin", new AdminDashboardOptions
                            {
                                Authorization = new[] { new AllowAllAdminDashboardAuthorizationFilter() },
                                DashboardTitle = "Test Admin",
                                RequireAuthentication = true,
                                CreateDefaultAdminUser = true,
                                DefaultAdminPassword = "admin",
                            });
                        });
                })
                .Start();
        }

        public IHost GetTestHost() => _host;

        public void Dispose()
        {
            try
            {
                var connectionString = string.Format(ConnectionStringTemplate, "master");
                var options = new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new TestDbContext(options);
                context.Database.ExecuteSqlRaw($"IF DB_ID('{_dbName}') IS NOT NULL BEGIN ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_dbName}]; END");
            }
            catch
            {
                // Best effort cleanup
            }

            _host?.Dispose();
        }
    }
}
