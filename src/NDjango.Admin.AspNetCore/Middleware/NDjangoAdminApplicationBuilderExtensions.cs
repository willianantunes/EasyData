using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NDjango.Admin.Services;
using NDjango.Admin.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class NDjangoAdminApplicationBuilderExtensions
    {

        public static IApplicationBuilder UseNDjangoAdmin(this IApplicationBuilder app, Action<NDjangoAdminOptions> optionsTuner = null)
        {
            return UseNDjangoAdmin<NDjangoAdminApiHandler>(app, optionsTuner);
        }

        public static IApplicationBuilder UseNDjangoAdmin<THandler>(this IApplicationBuilder app, Action<NDjangoAdminOptions> optionsTuner = null) where THandler : NDjangoAdminApiHandler
        {
            var options = new NDjangoAdminOptions();
            optionsTuner?.Invoke(options);
            return app.UseMiddleware<NDjangoAdminMiddleware<THandler>>(options);
        }
    }
}
