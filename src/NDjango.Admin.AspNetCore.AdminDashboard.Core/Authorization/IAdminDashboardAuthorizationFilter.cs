namespace NDjango.Admin.AspNetCore.AdminDashboard.Authorization
{
    public interface IAdminDashboardAuthorizationFilter
    {
        bool Authorize(AdminDashboardContext context);
    }
}
