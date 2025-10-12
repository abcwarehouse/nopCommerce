using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Core.Configuration;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void Configure(IApplicationBuilder application)
        {}

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISearchSpringService, SearchSpringService>();
        }

        public int Order => 10;
    }
}
