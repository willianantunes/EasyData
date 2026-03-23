using System.Threading;
using System.Threading.Tasks;

using NDjango.Admin.Services;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Services
{
    /// <summary>
    /// Creates search filters for entity list search queries.
    /// Provider-specific packages (e.g. EF Core, MongoDB) register their implementations.
    /// </summary>
    internal interface ISearchFilterFactory
    {
        Task<EasyFilter> CreateSearchFilterAsync(MetaData model, string searchQuery, CancellationToken ct = default);
    }
}
