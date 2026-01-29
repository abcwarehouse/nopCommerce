using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.AbcCore.Services.Custom;
using Nop.Plugin.Misc.AbcExportOrder.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.AbcExportOrder.Tasks
{
    public class ResubmitOrdersTask : IScheduleTask
    {
        private readonly IAbcOrderService _customOrderService;
        private readonly ExportOrderSettings _settings;
        private readonly ILogger _logger;
        private readonly IProductService _productService;

        public ResubmitOrdersTask(
            IAbcOrderService orderService,
            ExportOrderSettings settings,
            ILogger logger,
            IProductService productService
            )
        {
            _customOrderService = orderService;
            _settings = settings;
            _logger = logger;
            _productService = productService;
        }

        public async Task ExecuteAsync()
        {
            if (!_settings.IsValid)
            {
                throw new NopException("Unable to resubmit orders, export order settings not valid.");
            }

            var unsubmittedOrders = _customOrderService.GetUnsubmittedOrders();
            if (!unsubmittedOrders.Any())
            {
                return;
            }

            foreach (var order in unsubmittedOrders)
            {
                try
                {
                    await order.SubmitToISAMAsync();
                }
                catch (Exception e)
                {
                    await _logger.ErrorAsync($"Failure when resubmitting order #{order.Id}", e);
                    throw;
                }
            }
        }
    }
}
