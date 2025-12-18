using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbcExportOrder.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.AbcExportOrder
{
    public class NopStartup : INopStartup
    {
        public int Order => 1;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIsamOrderService, IsamOrderService>();
            services.AddScoped<IYahooService, YahooService>();
        }

        public void Configure(IApplicationBuilder application)
        {
            // No middleware configuration needed for this plugin
        }
    }
}
