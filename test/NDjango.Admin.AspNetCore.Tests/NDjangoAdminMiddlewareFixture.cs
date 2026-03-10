using System;
using System.Data.Common;

using Microsoft.Data.Sqlite;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NDjango.Admin.Services;
using Korzh.DbUtils;

namespace NDjango.Admin.AspNetCore.Tests
{

    public class NDjangoAdminMiddlewareFixture : IDisposable
    {

        private readonly IHost _host;

        private readonly SqliteConnection _connection;

        public NDjangoAdminMiddlewareFixture()
        {
            _connection = new SqliteConnection("Data Source=file::memory:");

            _host = new HostBuilder()
             .ConfigureWebHost(webBuilder =>
             {
                 webBuilder
                     .UseTestServer()
                     .ConfigureServices(services =>
                     {
                         services.AddEntityFrameworkSqlite();
                         services.AddDbContext<TestDbContext>(options =>
                         {
                             options.UseSqlite(_connection);
                             options.UseInternalServiceProvider(services.BuildServiceProvider());
                         });

                         services.AddRouting();
                     })
                     .Configure(app =>
                     {
                         app.UseNDjangoAdmin(options =>
                         {
                             options.UseDbContext<TestDbContext>();
                         });

                         app.UseRouting();

                         app.UseEndpoints(endpoints =>
                         {
                             endpoints.MapNDjangoAdmin((options) =>
                             {
                                 options.Endpoint = "/api/data";
                                 options.UseDbContext<TestDbContext>();
                             });
                         });

                         EnsureDbInitialized(app);
                     });
             })
             .Start();
        }

        private void EnsureDbInitialized(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<TestDbContext>())
            {
                context.Database.OpenConnection();
                if (context.Database.EnsureCreated())
                {
                    DbInitializer.Create(options =>
                    {
                        options.UseSqlite(_connection);
                        options.UseJsonImporter();
                        options.UseResourceFileUnpacker(typeof(NDjangoAdminMiddlewareFixture).Assembly, "Resources\\Nwind");
                    })
                    .Seed();
                }
            }
        }

        public IHost GetTestHost()
        {
            return _host;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
