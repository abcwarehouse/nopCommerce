﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Web.Framework.Infrastructure.Extensions;
using LinqToDB.Data;

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

            // Set global LinqToDB command timeout (seconds).
            // Temporary workaround for long-running queries in the admin
            DataConnection.DefaultCommandTimeout = 120;


            // Add the Permissions-Policy header for geolocation
            application.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    // Allow geolocation from your own site
                    context.Response.Headers["Permissions-Policy"] = "geolocation=(self)";
                    return Task.CompletedTask;
                });

                await next();
            });
            application.ConfigureRequestPipeline();
            application.StartEngine();
        }


    }
}