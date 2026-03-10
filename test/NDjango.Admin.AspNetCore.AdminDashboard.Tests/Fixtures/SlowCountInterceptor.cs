using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.Fixtures
{
    /// <summary>
    /// EF Core interceptor that injects a delay into COUNT queries,
    /// causing them to exceed the configured PaginationCountTimeoutMs.
    /// </summary>
    public class SlowCountInterceptor : DbCommandInterceptor
    {
        private readonly int _delayMs;

        public SlowCountInterceptor(int delayMs)
        {
            _delayMs = delayMs;
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("COUNT"))
            {
                await Task.Delay(_delayMs, cancellationToken);
            }

            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
