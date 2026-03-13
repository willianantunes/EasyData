using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NDjango.Admin;
using NDjango.Admin.AspNetCore.AdminDashboard;
using NDjango.Admin.AspNetCore.AdminDashboard.Authorization;
using NDjango.Admin.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.Authorization
{
    public class LocalRequestsOnlyAuthorizationFilterTests
    {
        private readonly LocalRequestsOnlyAuthorizationFilter _filter = new();

        private class StubAdminManager : NDjangoAdminManager
        {
            public StubAdminManager() : base(null, new NDjangoAdminOptions()) { }
            public override Task LoadModelAsync(string modelId, CancellationToken ct = default) => Task.CompletedTask;
            public override Task<NDjangoAdminResultSet> FetchDatasetAsync(string modelId, string sourceId, IEnumerable<EasyFilter> filters = null, IEnumerable<EasySorter> sorters = null, bool isLookup = false, int? offset = null, int? fetch = null, CancellationToken ct = default) => Task.FromResult(new NDjangoAdminResultSet());
            public override Task<long> GetTotalRecordsAsync(string modelId, string sourceId, IEnumerable<EasyFilter> filters = null, bool isLookup = false, CancellationToken ct = default) => Task.FromResult(0L);
            public override Task<object> FetchRecordAsync(string modelId, string sourceId, Dictionary<string, string> keys, CancellationToken ct = default) => Task.FromResult<object>(null);
            public override Task<object> CreateRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default) => Task.FromResult<object>(null);
            public override Task<object> UpdateRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default) => Task.FromResult<object>(null);
            public override Task DeleteRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default) => Task.CompletedTask;
            public override Task DeleteRecordsByKeysAsync(string modelId, string sourceId, IReadOnlyList<Dictionary<string, string>> recordKeysList, CancellationToken ct = default) => Task.CompletedTask;
            public override Task<IReadOnlyList<object>> FetchRecordsByKeysAsync(string modelId, string sourceId, IReadOnlyList<Dictionary<string, string>> recordKeysList, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<object>>(new List<object>());
            public override Task<IEnumerable<EasySorter>> GetDefaultSortersAsync(string modelId, string sourceId, CancellationToken ct = default) => Task.FromResult<IEnumerable<EasySorter>>(new List<EasySorter>());
        }

        private AdminDashboardContext CreateContext(IPAddress remoteIp, IPAddress localIp = null)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = remoteIp;
            httpContext.Connection.LocalIpAddress = localIp;

            return new AdminDashboardContext(httpContext, new AdminDashboardOptions(), new StubAdminManager(), "/admin");
        }

        [Fact]
        public void Authorize_LoopbackAddress_ReturnsTrue()
        {
            // Arrange
            var context = CreateContext(IPAddress.Loopback);

            // Act
            var result = _filter.Authorize(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authorize_IPv6Loopback_ReturnsTrue()
        {
            // Arrange
            var context = CreateContext(IPAddress.IPv6Loopback);

            // Act
            var result = _filter.Authorize(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authorize_RemoteMatchesLocal_ReturnsTrue()
        {
            // Arrange
            var ip = IPAddress.Parse("192.168.1.100");
            var context = CreateContext(ip, ip);

            // Act
            var result = _filter.Authorize(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authorize_NullRemoteIp_ReturnsFalse()
        {
            // Arrange
            var context = CreateContext(null);

            // Act
            var result = _filter.Authorize(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authorize_ExternalIp_ReturnsFalse()
        {
            // Arrange
            var context = CreateContext(IPAddress.Parse("8.8.8.8"), IPAddress.Parse("192.168.1.1"));

            // Act
            var result = _filter.Authorize(context);

            // Assert
            Assert.False(result);
        }
    }
}
