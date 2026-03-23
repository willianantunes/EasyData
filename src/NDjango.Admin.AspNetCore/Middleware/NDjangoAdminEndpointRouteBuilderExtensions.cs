using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

using NDjango.Admin.AspNetCore;
using NDjango.Admin.Services;

namespace Microsoft.AspNetCore.Builder
{
    public static class NDjangoAdminEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapNDjangoAdmin(
            this IEndpointRouteBuilder builder,
            Action<NDjangoAdminOptions> optionsTuner = null)
        {
            return MapNDjangoAdmin<NDjangoAdminApiHandler>(builder, optionsTuner);
        }


        public static IEndpointConventionBuilder MapNDjangoAdmin<THandler>(
            this IEndpointRouteBuilder builder,
            Action<NDjangoAdminOptions> optionsTuner = null) where THandler : NDjangoAdminApiHandler
        {
            var options = new NDjangoAdminOptions();
            optionsTuner?.Invoke(options);

            options.Endpoint = options.Endpoint.ToString().TrimEnd('/');

            var pattern = RoutePatternFactory.Parse(options.Endpoint + "/{**slug}");

            var app = builder.CreateApplicationBuilder();
            app.UseMiddleware<NDjangoAdminMiddleware<THandler>>(options);

            // return 404 if the request was not processed by NDjangoAdminMiddleware 
            // to prevent the exception on reaching the end of pipeline
            app.Run(context => {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });

            return builder.Map(pattern, app.Build())
                          .WithDisplayName("NDjango.Admin API");
        }
    }
}
