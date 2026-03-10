using System.Threading;
using System.Threading.Tasks;

namespace EasyData.AspNetCore.AdminDashboard.Authentication.Storage
{
    internal interface IAuthStorageInitializer
    {
        Task InitializeAsync(CancellationToken ct = default);
    }
}
