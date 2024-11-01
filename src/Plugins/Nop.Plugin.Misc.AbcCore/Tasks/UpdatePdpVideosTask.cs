using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Plugin.Misc.AbcCore.Services.Custom;
using Nop.Services.Catalog;
using Nop.Services.Tasks;
using Nop.Data;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.AbcCore.Tasks
{
    class UpdatePdpVideosTask : IScheduleTask
    {
        private readonly ILogger _logger;

        public UpdatePdpVideosTask(ILogger logger)
        {
            _logger = logger;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _logger.InformationAsync("Updating PDP videos.");

            await _logger.InformationAsync("PDP videos updated.");
        }
    }
}
