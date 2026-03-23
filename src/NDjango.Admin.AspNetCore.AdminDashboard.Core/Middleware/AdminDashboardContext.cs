using Microsoft.AspNetCore.Http;

using NDjango.Admin.Services;

namespace NDjango.Admin.AspNetCore.AdminDashboard
{
    public class AdminDashboardContext
    {
        public HttpContext HttpContext { get; }
        public AdminDashboardOptions Options { get; }
        public NDjangoAdminManager Manager { get; }
        public string BasePath { get; }
        public string AuthenticatedUsername { get; set; }
        public string AuthenticatedUserId { get; set; }
        public bool IsSuperuser { get; set; }

        public AdminDashboardContext(HttpContext httpContext, AdminDashboardOptions options, NDjangoAdminManager manager, string basePath)
        {
            HttpContext = httpContext;
            Options = options;
            Manager = manager;
            BasePath = basePath;
        }
    }
}
