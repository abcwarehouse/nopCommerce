// Nop.Web/Infrastructure/CustomMiddlewareStartup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Logging; // For ILogger
using Nop.Core.Domain.Logging; // For LogLevel enum
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
            
            var serviceProvider = application.ApplicationServices;
            var logger = serviceProvider.GetService<ILogger>();

            application.Use(async (context, next) =>
            {
                var goneUrls = new[]
                {
                    "/water-heaters-delivered-installed-within-24hours"
                };

                // Normalize path: lowercase + no trailing slash
                var requestPath = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();

                // Log the path being checked
                logger?.InsertLog(LogLevel.Information, $"Custom middleware checking path: {requestPath}");

                if (goneUrls.Any(u => string.Equals(u, requestPath, StringComparison.OrdinalIgnoreCase)))
                {
                    logger?.InsertLog(LogLevel.Information, $"410 Gone returned for path: {requestPath}");

                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    await context.Response.WriteAsync("410 Gone - This page has been permanently removed.");
                    return;
                }

                await next();
            });
        }
    }
}
