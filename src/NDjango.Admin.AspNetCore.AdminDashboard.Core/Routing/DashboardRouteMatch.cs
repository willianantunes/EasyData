using System.Collections.Generic;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Routing
{
    public class DashboardRouteMatch
    {
        public IDashboardDispatcher Dispatcher { get; set; }
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }
}
