using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using NDjango.Admin.MongoDB.Authentication.Entities;
using NDjango.Admin.MongoDB.Authentication.Storage;
using NDjango.Admin.MongoDB.Tests.Fixtures;
using Xunit;

namespace NDjango.Admin.MongoDB.Tests.IntegrationTests
{
    public class MongoCrudTests : IClassFixture<MongoAuthEnabledFixture>
    {
        private readonly HttpClient _client;
        private readonly MongoAuthEnabledFixture _fixture;

        public MongoCrudTests(MongoAuthEnabledFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.GetTestHost().GetTestClient();
        }

        [Fact]
        public async Task CreatePost_MongoAuthGroup_CreatesRecordAndRedirectsAsync()
        {
            // Arrange
            var cookie = await LoginAsAdminAsync();
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Name", "TestGroupCreate"),
                new KeyValuePair<string, string>("_save_action", "save"),
            });
            var request = new HttpRequestMessage(HttpMethod.Post, "/admin/MongoAuthGroup/add/")
            {
                Content = formData,
            };
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/admin/MongoAuthGroup/", response.Headers.Location.ToString());

            var database = _fixture.GetDatabase();
            var collection = database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var created = await collection.Find(g => g.Name == "TestGroupCreate").FirstOrDefaultAsync();
            Assert.NotNull(created);
        }

        [Fact]
        public async Task UpdatePost_MongoAuthGroup_UpdatesRecordAndRedirectsAsync()
        {
            // Arrange
            var database = _fixture.GetDatabase();
            var collection = database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var group = new MongoAuthGroup { Name = "TestGroupUpdate" };
            await collection.InsertOneAsync(group);

            var cookie = await LoginAsAdminAsync();
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Name", "TestGroupUpdated"),
                new KeyValuePair<string, string>("_save_action", "save"),
            });
            var request = new HttpRequestMessage(HttpMethod.Post, $"/admin/MongoAuthGroup/{group.Id}/change/")
            {
                Content = formData,
            };
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/admin/MongoAuthGroup/", response.Headers.Location.ToString());

            var updated = await collection.Find(g => g.Id == group.Id).FirstOrDefaultAsync();
            Assert.Equal("TestGroupUpdated", updated.Name);
        }

        [Fact]
        public async Task DeletePost_MongoAuthGroup_DeletesRecordAndRedirectsAsync()
        {
            // Arrange
            var database = _fixture.GetDatabase();
            var collection = database.GetCollection<MongoAuthGroup>(AuthCollectionNames.Groups);
            var group = new MongoAuthGroup { Name = "TestGroupDelete" };
            await collection.InsertOneAsync(group);

            var cookie = await LoginAsAdminAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"/admin/MongoAuthGroup/{group.Id}/delete/")
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()),
            };
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/admin/MongoAuthGroup/", response.Headers.Location.ToString());

            var deleted = await collection.Find(g => g.Id == group.Id).FirstOrDefaultAsync();
            Assert.Null(deleted);
        }

        [Fact]
        public async Task CreateAndVerifyInList_MongoAuthGroup_ShowsCreatedRecordAsync()
        {
            // Arrange
            var cookie = await LoginAsAdminAsync();
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Name", "TestGroupListVerify"),
                new KeyValuePair<string, string>("_save_action", "save"),
            });
            var createRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/MongoAuthGroup/add/")
            {
                Content = formData,
            };
            createRequest.Headers.Add("Cookie", cookie);

            // Act
            await _client.SendAsync(createRequest);
            var listRequest = new HttpRequestMessage(HttpMethod.Get, "/admin/MongoAuthGroup/");
            listRequest.Headers.Add("Cookie", cookie);
            var listResponse = await _client.SendAsync(listRequest);
            var html = await listResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
            Assert.Contains("TestGroupListVerify", html);
        }

        private async Task<string> LoginAsAdminAsync()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "admin"),
            });

            var loginResponse = await _client.PostAsync("/admin/login/", formContent);
            foreach (var header in loginResponse.Headers.GetValues("Set-Cookie"))
            {
                if (header.Contains(".NDjango.Admin.Auth"))
                    return header.Split(';')[0];
            }
            return null;
        }
    }
}
