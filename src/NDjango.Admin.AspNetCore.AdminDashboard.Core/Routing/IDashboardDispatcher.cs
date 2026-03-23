using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Routing
{
    public interface IDashboardDispatcher
    {
        Task DispatchAsync(AdminDashboardContext context, DashboardRouteMatch match);
    }
}
