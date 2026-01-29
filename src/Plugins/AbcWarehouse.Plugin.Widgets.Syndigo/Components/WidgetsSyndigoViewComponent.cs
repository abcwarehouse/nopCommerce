using Nop.Services.Logging;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Core;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Infrastructure;
using Nop.Services.Catalog;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Models;

namespace AbcWarehouse.Plugin.Widgets.Syndigo.Components
{
    public class WidgetsSyndigoViewComponent : NopViewComponent
    {
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        public WidgetsSyndigoViewComponent(
            ILogger logger,
            IProductService productService,
            IWebHelper webHelper
        )
        {
            _logger = logger;
            _productService = productService;
            _webHelper = webHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            if (widgetZone == CustomPublicWidgetZones.ProductDetailsDescriptionTabTop)
            {
                var productId = (additionalData as TabUIModel).ProductModel.Id;
                var product = await _productService.GetProductByIdAsync(productId);
                var fullDescription = product.FullDescription;

                // Only load Syndigo content if RWS content is not in full description
                return !fullDescription.Contains("<div class=\"basic-overview\">") ?
                    View("~/Plugins/Widgets.Syndigo/Views/ContentDisplay.cshtml") :
                    Content("");
            }

            await _logger.WarningAsync($"Widgets.Syndigo: No view provided for widget zone {widgetZone}");
            return Content("");
        }
    }
}
