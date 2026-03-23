using System.Collections.Generic;

namespace NDjango.Admin.AspNetCore.AdminDashboard.ViewModels
{
    public class BulkDeleteViewModel
    {
        public string Title { get; set; }
        public string BasePath { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityNamePlural { get; set; }
        public List<string> SelectedIds { get; set; } = new List<string>();
        public List<Dictionary<string, object>> SelectedRecords { get; set; } = new List<Dictionary<string, object>>();
        public Dictionary<string, List<EntityGroupItem>> SidebarGroups { get; set; }
    }
}
