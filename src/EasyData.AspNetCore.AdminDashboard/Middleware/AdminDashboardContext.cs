using Microsoft.AspNetCore.Http;

using EasyData.Services;

namespace EasyData.AspNetCore.AdminDashboard
{
    public class AdminDashboardContext
    {
        public HttpContext HttpContext { get; }
        public AdminDashboardOptions Options { get; }
        public EasyDataManager Manager { get; }
        public string BasePath { get; }
        public string AuthenticatedUsername { get; set; }
        public int AuthenticatedUserId { get; set; }
        public bool IsSuperuser { get; set; }

        public AdminDashboardContext(HttpContext httpContext, AdminDashboardOptions options, EasyDataManager manager, string basePath)
        {
            HttpContext = httpContext;
            Options = options;
            Manager = manager;
            BasePath = basePath;
        }
    }
}
