using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Services.Custom;
using Nop.Web.Framework.Infrastructure.Extensions;
using Autofac;

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
            services.AddScoped<IListrakApiService, ListrakApiService>();
            services.AddHttpClient<ListrakApiService>();
            services.AddHttpClient();

            // Bind ListrakApiSettings to configuration
            services.Configure<ListrakApiSettings>(_configuration.GetSection("ListrakApi"));
        }

        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            application.ConfigureRequestPipeline();
            application.StartEngine();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register ListrakApiSettings
            builder.Register(ctx =>
            {
                var config = ctx.Resolve<IConfiguration>();
                var listrakApiSettings = new ListrakApiSettings();
                config.GetSection("ListrakApi").Bind(listrakApiSettings);
                return listrakApiSettings;
            }).SingleInstance();

            // Register ListrakApiService
            builder.RegisterType<ListrakApiService>().AsSelf().InstancePerLifetimeScope();
        }
    }
}
