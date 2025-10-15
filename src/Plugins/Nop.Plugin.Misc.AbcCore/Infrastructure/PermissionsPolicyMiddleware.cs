using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Misc.AbcCore.Infrastructure
{
    public class PermissionsPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionsPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                // Allow geolocation from your own site
                context.Response.Headers["Permissions-Policy"] = "geolocation=(self)";
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}