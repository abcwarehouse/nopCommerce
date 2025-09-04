// Nop.Web/Infrastructure/CustomMiddlewareStartup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            
            application.Use(async (context, next) =>
            {
                // Log to console to confirm middleware runs
                Console.WriteLine($"Custom middleware checking path: {context.Request.Path.Value}");

                var goneUrls = new[]
                {
                    "/water-heaters-delivered-installed-within-24hours"
                };

                // Normalize path: lowercase + no trailing slash
                var requestPath = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();

                if (goneUrls.Any(u => string.Equals(u, requestPath, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    context.Response.ContentType = "text/html";

                     await context.Response.WriteAsync(@"
                            <html>
                                  <head>
                                      <title>Page Removed</title>
                                      <meta http-equiv='refresh' content='3;url=https://www.abcwarehouse.com/' />
                                 </head>
                                         <body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>
                                             <h1>410 - This page has been permanently removed</h1>
                                             <p>You will be redirected to our <a href='https://www.abcwarehouse.com/'>homepage</a> in 3 seconds.</p>
                                        </body>
                            </html>
                ");
                 return;
        }

                await next();
            });
        }
    }
}
