// Nop.Web/Infrastructure/CustomMiddlewareStartup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // <-- Use Microsoft logging
using Nop.Core.Infrastructure;
using System;
using System.Linq;

namespace Nop.Web.Infrastructure
{
    public class CustomMiddlewareStartup : INopStartup
    {
        // Run early in the pipeline
        public int Order => 1;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // No services to configure
        }

        public void Configure(IApplicationBuilder application)
        {
            // Get Microsoft logger
            var logger = application.ApplicationServices.GetService<ILogger<CustomMiddlewareStartup>>();

            application.Use(async (context, next) =>
            {
                var goneUrls = new[]
                {
                    "/water-heaters-delivered-installed-within-24hours"
                };

                // Normalize path
                var requestPath = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();

                // Log that middleware ran
                logger?.LogInformation("Custom middleware checking path: {Path}", requestPath);

                if (goneUrls.Any(u => string.Equals(u, requestPath, StringComparison.OrdinalIgnoreCase)))
                {
                    logger?.LogInformation("410 Gone returned for path: {Path}", requestPath);

                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    await context.Response.WriteAsync("410 Gone - This page has been permanently removed.");
                    return;
                }

                await next();
            });
        }
    }
}
