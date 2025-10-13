using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using AbcWarehouse.Plugin.Widgets.Listrak;

namespace AbcWarehouse.Plugin.Widgets.Listrak
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IListrakService, ListrakService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;
    }
}
