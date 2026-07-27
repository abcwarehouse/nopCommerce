using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbcEventSurveys.Services;

namespace Nop.Plugin.Misc.AbcEventSurveys.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 1000;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISurveyEventService, SurveyEventService>();
            services.AddScoped<ISurveyExportService, SurveyExportService>();
        }

        public void Configure(IApplicationBuilder application)
        {
            // No middleware configuration needed for this plugin
        }
    }
}
