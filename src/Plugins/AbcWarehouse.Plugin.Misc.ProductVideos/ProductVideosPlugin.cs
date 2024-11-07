using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using AbcWarehouse.Plugin.Misc.ProductVideos.Tasks;
using Nop.Core.Domain.Tasks;
using Nop.Services.Tasks;

namespace AbcWarehouse.Plugin.Misc.ProductVideos
{
    public class ProductVideosPlugin : BasePlugin, IMiscPlugin
    {
        private readonly string _taskType = $"{typeof(UpdatePdpVideosTask).FullName}, {typeof(ProductVideosPlugin).Namespace}";

        private readonly IScheduleTaskService _scheduleTaskService;

        public ProductVideosPlugin(
            IScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
        }

        public override async System.Threading.Tasks.Task InstallAsync()
        {
            await RemoveTaskAsync();
            await AddTaskAsync();

            await base.InstallAsync();
        }

        public override async System.Threading.Tasks.Task UninstallAsync()
        {
            await RemoveTaskAsync();

            await base.UninstallAsync();
        }

        private async System.Threading.Tasks.Task AddTaskAsync()
        {
            ScheduleTask task = new ScheduleTask
            {
                Name = $"Update PDP Videos",
                Seconds = 14400,
                Type = _taskType,
                Enabled = true,
                StopOnError = false,
            };

            await _scheduleTaskService.InsertTaskAsync(task);
        }

        private async System.Threading.Tasks.Task RemoveTaskAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(_taskType);
            if (task != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(task);
            }
        }
    }
}
