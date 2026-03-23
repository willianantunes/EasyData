namespace NDjango.Admin.AspNetCore.AdminDashboard.Authorization
{
    public interface IAdminDashboardAuthorizationFilter
    {
        public bool Authorize(AdminDashboardContext context);
    }
}
