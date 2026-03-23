using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Routing
{
    public interface IDashboardDispatcher
    {
        public Task DispatchAsync(AdminDashboardContext context, DashboardRouteMatch match);
    }
}
