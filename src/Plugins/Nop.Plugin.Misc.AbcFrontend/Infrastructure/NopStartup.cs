using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbcFrontend.Services;
using Nop.Services.Tax;
using Microsoft.Extensions.DependencyInjection;
using Nop.Services.Orders;
using Microsoft.Extensions.Configuration;               // IConfiguration
using Microsoft.AspNetCore.Builder;                     // IApplicationBuilder

namespace Nop.Plugin.Misc.AbcFrontend.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IWarrantyService, WarrantyService>();
        }

        public int Order => 2;
        
        public void Configure(IApplicationBuilder application) {}
    }
}
