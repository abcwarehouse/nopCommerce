using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Tasks;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.AbcCore.Tasks
{
    /// <summary>
    /// Periodically removes old PageNotFoundRecord entries using a single SQL DELETE.
    /// </summary>
    public class PageNotFoundRecordCleanTask : IScheduleTask
    {
        private readonly INopDataProvider _dataProvider;
        private readonly ILogger _logger;

        // Retention in days. You can replace this with a setting if needed.
        private const int DefaultRetentionDays = 7;

        public PageNotFoundRecordCleanTask(
            INopDataProvider dataProvider,
            ILogger logger)
        {
            _dataProvider = dataProvider;
            _logger = logger;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            try
            {
                var sql = $"DELETE FROM [PageNotFoundRecord] WHERE CreatedOnUtc < DATEADD(day, -{DefaultRetentionDays}, GETDATE());";
                var affected = await _dataProvider.ExecuteNonQueryAsync(sql);
                await _logger.InformationAsync($"PageNotFoundRecordCleanTask: removed {affected} records older than {DefaultRetentionDays} days.");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("PageNotFoundRecordCleanTask: unexpected error during cleanup.", ex);
            }
        }
    }
}
