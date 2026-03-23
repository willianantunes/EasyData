using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Storage
{
    internal interface IAuthStorageInitializer
    {
        Task InitializeAsync(CancellationToken ct = default);
    }
}
