using AbcWarehouse.Plugin.Misc.DealTherapy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDealTherapyService, DealTherapyService>();
        }

        public void Configure(IApplicationBuilder application) { }

        public int Order => 2;
    }
}
