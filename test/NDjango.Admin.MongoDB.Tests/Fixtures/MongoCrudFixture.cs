using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Bson;
using MongoDB.Driver;

using NDjango.Admin.AspNetCore.AdminDashboard;
using NDjango.Admin.AspNetCore.AdminDashboard.Authorization;

namespace NDjango.Admin.MongoDB.Tests.Fixtures
{
    public class MongoCrudFixture : IDisposable
    {
        private readonly IHost _host;
        private readonly string _dbName;
        private readonly IMongoClient _mongoClient;

        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017/?directConnection=true";

        /// <summary>
        /// The ObjectId of the seeded category ("Italian").
        /// </summary>
        public ObjectId SeededCategoryId { get; private set; }

        public MongoCrudFixture()
        {
            _dbName = $"NDjangoAdminMongoCrudTest_{Guid.NewGuid():N}";
            _mongoClient = new MongoClient(ConnectionString);
            var database = _mongoClient.GetDatabase(_dbName);

            SeedDatabase(database);

            _host = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IMongoClient>(_mongoClient);
                            services.AddSingleton<IMongoDatabase>(database);

                            services.AddNDjangoAdminDashboardMongo(
                                new AdminDashboardOptions
                                {
                                    Authorization = new[] { new AllowAllAdminDashboardAuthorizationFilter() },
                                    DashboardTitle = "Test Mongo CRUD Admin",
                                },
                                mongo =>
                                {
                                    mongo.AddCollection<TestCategory>("categories");
                                    mongo.AddCollection<TestRestaurant>("restaurants");
                                    mongo.AddCollection<TestIngredient>("ingredients", readOnly: true);
                                    mongo.AddCollection<TestDocumentWithCreatedDate>("documents_with_created_date");
                                }
                            );
                        })
                        .Configure(app =>
                        {
                            app.UseNDjangoAdminDashboard("/admin");
                        });
                })
                .Start();
        }

        private void SeedDatabase(IMongoDatabase database)
        {
            var categories = database.GetCollection<TestCategory>("categories");
            var cat1 = new TestCategory { Name = "Italian", Description = "Italian cuisine" };
            categories.InsertOne(cat1);
            SeededCategoryId = cat1.Id;

            var restaurants = database.GetCollection<TestRestaurant>("restaurants");
            var rest1 = new TestRestaurant { Name = "Bella Roma", Address = "123 Main St" };
            restaurants.InsertOne(rest1);

            var ingredients = database.GetCollection<TestIngredient>("ingredients");
            var ing1 = new TestIngredient { Name = "Tomato", IsAllergen = false };
            ingredients.InsertOne(ing1);
        }

        public IHost GetTestHost() => _host;

        public IMongoDatabase GetDatabase() => _mongoClient.GetDatabase(_dbName);

        public void Dispose()
        {
            try
            {
                _mongoClient.DropDatabase(_dbName);
            }
            catch
            {
                // Best effort cleanup
            }

            _host?.Dispose();
        }
    }
}
