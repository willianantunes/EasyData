using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.TestHost;

using Xunit;

using EasyData.AspNetCore.AdminDashboard.Tests.Fixtures;

namespace EasyData.AspNetCore.AdminDashboard.Tests.AuthenticationTests
{
    public class PermissionSeederTests : IClassFixture<AuthEnabledFixture>
    {
        private readonly HttpClient _client;

        public PermissionSeederTests(AuthEnabledFixture fixture)
        {
            _client = fixture.GetTestHost().GetTestClient();
        }

        [Fact]
        public async Task GetDashboard_Authenticated_ShowsAuthGroupAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "/admin/");
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Authentication and Authorization", content);
        }

        [Fact]
        public async Task GetPermissionList_AsSuperuser_ShowsUserEntityPermissionsAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "/admin/AuthPermission/");
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("add_category", content);
            Assert.Contains("view_category", content);
            Assert.Contains("change_category", content);
            Assert.Contains("delete_category", content);
        }

        [Fact]
        public async Task GetPermissionList_AsSuperuser_ShowsAuthEntityPermissionsAsync()
        {
            // Arrange
            var cookie = await LoginAndGetCookieAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "/admin/AuthPermission/");
            request.Headers.Add("Cookie", cookie);

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("add_authgroup", content);
            Assert.Contains("view_authgroup", content);
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
                if (header.Contains(".EasyData.Admin.Auth"))
                    return header.Split(';')[0];
            }
            return null;
        }
    }
}
