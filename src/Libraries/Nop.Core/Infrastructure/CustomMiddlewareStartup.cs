// Nop.Web/Infrastructure/CustomMiddlewareStartup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure; 
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Nop.Web.Infrastructure
{
    public class CustomMiddlewareStartup : INopStartup // <-- Implement the interface
    {
        
        /// Gets order of this startup configuration implementation
        
        public int Order => 105; // Run after static files

    
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // No services to configure
        }

       
        /// Configure the using of added middleware
       
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            // Add the custom middleware to the pipeline
            application.Use(async (context, next) =>
            {
                var goneUrls = new[] { "/water-heaters-delivered-installed-within-24hours" };

                if (goneUrls.Contains(context.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    await context.Response.WriteAsync("410 Gone - This page has been permanently removed.");
                    return;
                }

                await next();
            });
        }
    }
}