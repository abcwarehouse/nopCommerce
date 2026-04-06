using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.AbcCore.Tasks
{
    public class UpdateProductVideosFromRwsTask : IScheduleTask
    {
        private readonly INopDataProvider _nopDataProvider;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UpdateProductVideosFromRwsTask(
            INopDataProvider nopDataProvider,
            ILogger logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _nopDataProvider = nopDataProvider;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            try
            {
                var sqlFilePath = Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "Plugins",
                    "Misc.AbcCore",
                    "Tasks",
                    "Sql",
                    "UpdateProductVideosFromRws.sql");

                if (!File.Exists(sqlFilePath))
                {
                    await _logger.WarningAsync($"UpdateProductVideosFromRwsTask: SQL file not found at '{sqlFilePath}'.");
                    return;
                }

                var sql = await File.ReadAllTextAsync(sqlFilePath);
                await _nopDataProvider.ExecuteNonQueryAsync(sql);

                await _logger.InformationAsync("UpdateProductVideosFromRws: SQL script executed successfully.");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("UpdateProductVideosFromRwsTask: failed to execute SQL script.", ex);
            }
        }
    }
}
