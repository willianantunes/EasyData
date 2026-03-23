using System;
using System.Collections.Generic;

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
    public class MongoDashboardFixture : IDisposable
    {
        private readonly IHost _host;
        private readonly string _dbName;
        private readonly IMongoClient _mongoClient;

        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017/?directConnection=true";

        /// <summary>
        /// The ObjectId of the first seeded category ("Italian").
        /// </summary>
        public ObjectId ItalianCategoryId { get; private set; }

        /// <summary>
        /// The ObjectId of the second seeded category ("Japanese").
        /// </summary>
        public ObjectId JapaneseCategoryId { get; private set; }

        /// <summary>
        /// The ObjectId of the third seeded category ("Mexican").
        /// </summary>
        public ObjectId MexicanCategoryId { get; private set; }

        public MongoDashboardFixture()
        {
            _dbName = $"NDjangoAdminMongoTest_{Guid.NewGuid():N}";
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
                                    DashboardTitle = "Test Mongo Admin",
                                },
                                mongo =>
                                {
                                    mongo.AddCollection<TestCategory>("categories");
                                    mongo.AddCollection<TestRestaurant>("restaurants");
                                    mongo.AddCollection<TestIngredient>("ingredients");
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
            var cat2 = new TestCategory { Name = "Japanese", Description = "Japanese cuisine" };
            var cat3 = new TestCategory { Name = "Mexican", Description = "Mexican cuisine" };
            categories.InsertMany(new[] { cat1, cat2, cat3 });

            ItalianCategoryId = cat1.Id;
            JapaneseCategoryId = cat2.Id;
            MexicanCategoryId = cat3.Id;

            var restaurants = database.GetCollection<TestRestaurant>("restaurants");
            var rest1 = new TestRestaurant { Name = "Bella Roma", Address = "123 Main St" };
            var rest2 = new TestRestaurant { Name = "Sakura", Address = "456 Oak Ave" };
            restaurants.InsertMany(new[] { rest1, rest2 });

            var ingredients = database.GetCollection<TestIngredient>("ingredients");
            var ing1 = new TestIngredient { Name = "Tomato", IsAllergen = false };
            var ing2 = new TestIngredient { Name = "Mozzarella", IsAllergen = true };
            var ing3 = new TestIngredient { Name = "Basil", IsAllergen = false };
            ingredients.InsertMany(new[] { ing1, ing2, ing3 });
        }

        public IHost GetTestHost() => _host;

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
