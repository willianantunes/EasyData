using Xunit;

namespace EasyData.AspNetCore.AdminDashboard.Tests.Fixtures
{
    [CollectionDefinition("SlowCount")]
    public class SlowCountCollection : ICollectionFixture<SlowCountFixture>
    {
    }
}
