using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using NDjango.Admin.MongoDB.Tests.Fixtures;
using Xunit;

namespace NDjango.Admin.MongoDB.Tests.IntegrationTests
{
    public class MongoLoginTests : IClassFixture<MongoAuthEnabledFixture>
    {
        private readonly HttpClient _client;

        public MongoLoginTests(MongoAuthEnabledFixture fixture)
        {
            _client = fixture.GetTestHost().GetTestClient();
        }

        [Fact]
        public async Task UnauthenticatedRequest_DashboardHome_RedirectsToLoginAsync()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/admin/");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/admin/login/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task GetLogin_NoAuth_Returns200WithFormAsync()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/admin/login/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Log in", content);
            Assert.Contains("username", content);
            Assert.Contains("password", content);
        }

        [Fact]
        public async Task PostLogin_ValidCredentials_RedirectsToDashboardAsync()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "admin"),
            });

            // Act
            var response = await _client.PostAsync("/admin/login/", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/admin/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task PostLogin_InvalidCredentials_ShowsErrorMessageAsync()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "wrong"),
            });

            // Act
            var response = await _client.PostAsync("/admin/login/", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Please enter the correct username and password", content);
        }

        [Fact]
        public async Task PostLogin_NonexistentUser_ShowsErrorAsync()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "nonexistent"),
                new KeyValuePair<string, string>("password", "test"),
            });

            // Act
            var response = await _client.PostAsync("/admin/login/", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Please enter the correct username and password", content);
        }

        [Fact]
        public async Task PostLogin_WithNextUrl_RedirectsToNextAsync()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "admin"),
                new KeyValuePair<string, string>("next", "/admin/TestCategory/"),
            });

            // Act
            var response = await _client.PostAsync("/admin/login/", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/admin/TestCategory/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task GetDashboard_Authenticated_ShowsWelcomeAndLogoutAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "/admin/");
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Welcome,", content);
            Assert.Contains("admin", content);
            Assert.Contains("Log out", content);
        }

        [Fact]
        public async Task GetLogout_Authenticated_RedirectsToLoginAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var logoutRequest = new HttpRequestMessage(HttpMethod.Get, "/admin/logout/");
            logoutRequest.Headers.Add("Cookie", cookie);

            // Act
            var logoutResponse = await _client.SendAsync(logoutRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);
            Assert.Equal("/admin/login/", logoutResponse.Headers.Location.ToString());
        }

        [Fact]
        public async Task GetCss_NoAuth_Returns200Async()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/admin/css/admin-dashboard.css");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetDashboard_Authenticated_ShowsAuthEntitiesAndUserEntitiesAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "/admin/");
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // User entities should be visible
            Assert.Contains("Test Categories", content);
            Assert.Contains("Test Restaurants", content);
            // Auth entities should be visible
            Assert.Contains("Mongo Auth Users", content);
            Assert.Contains("Mongo Auth Groups", content);
            Assert.Contains("Mongo Auth Permissions", content);
        }

        private async Task<string> LoginAndGetCookieAsync()
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
