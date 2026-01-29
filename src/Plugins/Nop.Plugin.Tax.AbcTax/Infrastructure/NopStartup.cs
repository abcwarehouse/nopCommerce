using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Tax.AbcTax.Services;
using Nop.Services.Tax;
using Nop.Services.Orders;

namespace Nop.Plugin.Tax.AbcTax.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 1;

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITaxProvider, AbcTaxProvider>();
            services.AddScoped<IAbcTaxService, AbcTaxService>();
            services.AddScoped<ITaxjarRateService, TaxjarRateService>();
            services.AddScoped<IWarrantyTaxService, WarrantyTaxService>();
            services.AddScoped<IOrderProcessingService, CustomOrderProcessingService>();
            services.AddScoped<IOrderTotalCalculationService, CustomOrderTotalCalculationService>();
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