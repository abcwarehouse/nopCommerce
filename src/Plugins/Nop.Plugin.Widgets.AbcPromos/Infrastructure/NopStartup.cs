using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.AbcPromos.Tasks;
using Nop.Plugin.Widgets.AbcPromos.Tasks.LegacyTasks;

namespace Nop.Plugin.Widgets.AbcPromos
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<UpdatePromosTask, UpdatePromosTask>();
            services.AddScoped<GenerateRebatePromoPageTask, GenerateRebatePromoPageTask>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            // Nothing to configure for this plugin
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
