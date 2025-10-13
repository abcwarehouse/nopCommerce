using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using AbcWarehouse.Plugin.Payments.UniFi.Services;

namespace AbcWarehouse.Plugin.Payments.UniFi
{
    public class NopStartup : INopStartup
    {
        public int Order => int.MaxValue;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITransactionLookupService, TransactionLookupService>();
        }

        public void Configure(IApplicationBuilder application)
        {
            // No additional middleware configuration needed
        }
    }
}
