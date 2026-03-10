using Xunit;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.Fixtures
{
    [CollectionDefinition("SlowCount")]
    public class SlowCountCollection : ICollectionFixture<SlowCountFixture>
    {
    }
}
