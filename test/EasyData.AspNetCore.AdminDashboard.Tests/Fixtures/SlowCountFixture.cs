using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using EasyData.AspNetCore.AdminDashboard.Authorization;

namespace EasyData.AspNetCore.AdminDashboard.Tests.Fixtures
{
    public class SlowCountFixture : IDisposable
    {
        private readonly IHost _host;
        private readonly string _dbName;

        private const string ConnectionStringTemplate =
            "Server=localhost,1433;Database={0};User Id=sa;Password=Password1;TrustServerCertificate=true;";

        /// <summary>
        /// Timeout set low enough so the interceptor delay (500ms) always exceeds it.
        /// </summary>
        public const int PaginationCountTimeoutMs = 50;

        /// <summary>
        /// Delay injected into COUNT queries by the interceptor — must exceed PaginationCountTimeoutMs.
        /// </summary>
        public const int InterceptorDelayMs = 500;

        public static readonly long ExpectedFallbackCount = EasyData.Services.EasyDataOptions.PaginationCountFallbackValue;

        public SlowCountFixture()
        {
            _dbName = $"EasyDataSlowCountTest_{Guid.NewGuid():N}";
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
                                options.AddInterceptors(new SlowCountInterceptor(InterceptorDelayMs));
                            });

                            services.AddEasyDataAdminDashboard<TestDbContext>();
                        })
                        .Configure(app =>
                        {
                            app.UseEasyDataAdminDashboard("/admin", new AdminDashboardOptions
                            {
                                Authorization = new[] { new AllowAllAdminDashboardAuthorizationFilter() },
                                DashboardTitle = "Test Admin",
                                PaginationCountTimeoutMs = PaginationCountTimeoutMs,
                            });

                            SeedDatabase(app);
                        });
                })
                .Start();
        }

        private void SeedDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

            context.Database.EnsureCreated();

            var cat1 = new Category { Name = "Italian", Description = "Italian cuisine" };
            context.Categories.Add(cat1);
            context.SaveChanges();

            var rest1 = new Restaurant { Name = "Bella Roma", Address = "123 Main St", CategoryId = cat1.Id };
            context.Restaurants.Add(rest1);
            context.SaveChanges();
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
