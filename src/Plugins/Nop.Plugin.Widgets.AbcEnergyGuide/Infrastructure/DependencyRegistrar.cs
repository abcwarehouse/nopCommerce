using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.AbcEnergyGuide.Services;

namespace Nop.Plugin.Widgets.AbcEnergyGuide.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => int.MaxValue;

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProductEnergyGuideService, ProductEnergyGuideService>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            // Nothing to configure for this plugin
        }
    }
}
