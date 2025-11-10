using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.AbcCore.Nop;

namespace AbcWarehouse.Plugin.Widgets.PercentOffAppliancesMessageViewComponent.Components
{
    [ViewComponent(Name = "PercentOffAppliancesMessage")]
    public class PercentOffAppliancesMessageViewComponent : NopViewComponent
    {
        private readonly ILogger _logger;
        private readonly IAbcCategoryService _abcCategoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;

        private static readonly string[] LGBuyMoreBrands = { 
            "LG SIGNATURE", "LG STUDIO", "LG XBOOM", "LG", "LG APPLIANCES", 
        };
        private static readonly string[] BuyMoreBrands = {
            "\"C\" BY GE", "GE CAFE", "GE MONOGRAM", "GE PROFILE", "GE", "Samsung", "FRIGIDAIRE", "Frigidaire Gallery",
            "Electrolux", "Electrolux ICON", "Electrolux Professional", "Miele", "Maytag", "Whirlpool",
        };
        private static readonly string[] ExcludedBrands = {
            "LG SIGNATURE", "LG STUDIO", "LG XBOOM", "\"C\" BY GE", "GE CAFE", "GE MONOGRAM",
            "GE PROFILE", "MONOGRAM", "MIELE", "DACOR", "JENN-AIR", "SUBZERO", "THERMADOR",
            "VIKING", "WOLF", "FULGOR", "SIGNATURE KITCHEN SUITE", "COVE", "BLUE STAR", "BOSCH"
        };

        public PercentOffAppliancesMessageViewComponent(
            ILogger logger,
            IAbcCategoryService abcCategoryService,
            IManufacturerService manufacturerService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext)
        {
            _logger = logger;
            _abcCategoryService = abcCategoryService;
            _manufacturerService = manufacturerService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, int additionalData)
        {
            // Don't display for Hawthorne store
            if ((await _storeContext.GetCurrentStoreAsync()).Id == 8)
                return Content("");

            var productId = additionalData;
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                await _logger.WarningAsync($"Percent Off Appliances Message Widget: Unable to find product with ID '{productId}'.");
                return Content("");
            }

            if (product.CallForPrice || await product.IsAddToCartToSeePriceAsync())
                return Content("");

            // Check if product belongs to an appliance category
            bool isAppliance = await HasApplianceCategoryAsync(productId);
            if (!isAppliance)
                return Content("");

            // Determine manufacturer
            var manufacturer = await GetPrimaryManufacturerAsync(productId);
            if (manufacturer == null || !manufacturer.Published)
                return Content("");

            // Brand-based message logic
            var formattedDiscount = await GetFormattedDiscountedPriceAsync(product.Price);
            var discountedPrice = await _priceCalculationService.RoundPriceAsync(product.Price * 0.95M);

            if (LGBuyMoreBrands.Contains(manufacturer.Name))
                return View("~/Plugins/Widgets.PercentOffAppliancesMessage/Views/LGBMSMMessage.cshtml", discountedPrice);

            if (BuyMoreBrands.Contains(manufacturer.Name))
                return View("~/Plugins/Widgets.PercentOffAppliancesMessage/Views/BMSMMessage.cshtml", discountedPrice);

            if (ExcludedBrands.Contains(manufacturer.Name))
                return Content("");

            // Default message
            return View("~/Plugins/Widgets.PercentOffAppliancesMessage/Views/Message.cshtml", formattedDiscount);
        }

        /// <summary>
        /// Checks if the product is under an appliance category.
        /// </summary>
        private async Task<bool> HasApplianceCategoryAsync(int productId)
        {
            var categories = await _abcCategoryService.GetProductCategoriesByProductIdAsync(productId);
            foreach (var category in categories)
            {
                if (await _abcCategoryService.HasApplianceTopLevelCategoryAsync(category.CategoryId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the first manufacturer associated with a product.
        /// </summary>
        private async Task<Manufacturer> GetPrimaryManufacturerAsync(int productId)
        {
            var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(productId);
            if (!productManufacturers.Any())
                return null;

            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(productManufacturers.First().ManufacturerId);
            return manufacturer;
        }

        /// <summary>
        /// Calculates and formats the discounted price (10% off).
        /// </summary>
        private async Task<string> GetFormattedDiscountedPriceAsync(decimal originalPrice)
        {
            var discounted = await _priceCalculationService.RoundPriceAsync(originalPrice * 0.9M);
            return await _priceFormatter.FormatPriceAsync(discounted);
        }
    }
}
