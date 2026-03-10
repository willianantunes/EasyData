namespace NDjango.Admin.AspNetCore.AdminDashboard.Authorization
{
    public class AllowAllAdminDashboardAuthorizationFilter : IAdminDashboardAuthorizationFilter
    {
        public bool Authorize(AdminDashboardContext context) => true;
    }
}
