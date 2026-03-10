using Xunit;

namespace EasyData.AspNetCore.AdminDashboard.Tests.DashboardTests
{
    public class AdminDashboardOptionsTests
    {
        [Fact]
        public void PaginationCountTimeoutMs_DefaultValue_Is200()
        {
            // Arrange & Act
            var options = new AdminDashboardOptions();

            // Assert
            Assert.Equal(200, options.PaginationCountTimeoutMs);
        }
    }
}
