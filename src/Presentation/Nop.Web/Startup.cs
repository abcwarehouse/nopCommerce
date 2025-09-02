using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Services.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Nop.Web
{
    /// <summary>
    /// Represents startup class of application
    /// </summary>
    public class Startup
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region Ctor

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        #endregion

        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureApplicationServices(_configuration, _webHostEnvironment);
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
      


        public void Configure(IApplicationBuilder application)
        {
            application.ConfigureRequestPipeline();
            application.StartEngine();

            // Create a scoped service provider to get nopCommerce services
            var serviceProvider = application.ApplicationServices;
            var logger = serviceProvider.GetService<ILogger>();

            // 410 error with nopCommerce DB logging
            application.Use(async (context, next) =>
            {
                var goneUrls = new[] { "/water-heaters-delivered-installed-within-24hours" };

                if (goneUrls.Contains(context.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
                {
                    // Log to nopCommerce DB
                    logger?.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, 
                     $"410 Gone - URL: {context.Request.Path.Value} | IP: {context.Connection.RemoteIpAddress}");

                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    await context.Response.WriteAsync("410 Gone - This page has been permanently removed.");
                    return;
                }

                await next();
            });
        }

    }
}