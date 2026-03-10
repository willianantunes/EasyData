using System;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.Fixtures
{
    internal static class TestConnectionHelper
    {
        private static readonly string Server =
            Environment.GetEnvironmentVariable("TEST_DB_SERVER") ?? "localhost,1433";

        internal static readonly string ConnectionStringTemplate =
            $"Server={Server};Database={{0}};User Id=sa;Password=Password1;TrustServerCertificate=true;";
    }
}
