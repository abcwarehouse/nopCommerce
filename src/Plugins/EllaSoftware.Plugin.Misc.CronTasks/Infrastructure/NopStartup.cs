using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using EllaSoftware.Plugin.Misc.CronTasks.Services;
using Quartz;
using Quartz.Impl;
using System;

namespace EllaSoftware.Plugin.Misc.CronTasks.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICronTaskService, CronTaskService>();

            // quartz
            services.AddSingleton<StdSchedulerFactory>();
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            services.AddSingleton<IScheduler>(scheduler);
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;
    }
}
