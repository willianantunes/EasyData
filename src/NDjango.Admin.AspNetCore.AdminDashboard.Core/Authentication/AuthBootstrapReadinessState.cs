using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication
{
    internal class AuthBootstrapReadinessState
    {
        private readonly TaskCompletionSource<bool> _readyTcs =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsReady => _readyTcs.Task.IsCompleted && _readyTcs.Task.Result;

        public Task WaitForReadyAsync(CancellationToken ct = default)
            => _readyTcs.Task.WaitAsync(ct);

        public void SetReady() => _readyTcs.TrySetResult(true);

        public void SetFailed() => _readyTcs.TrySetResult(false);
    }
}
