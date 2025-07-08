using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Nop;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout.Components
{
    public class CartSlideoutViewComponent : NopViewComponent
    {
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IAbcCategoryService _abcCategoryService;
        private readonly IProductService _productService;

        public CartSlideoutViewComponent(
            IShoppingCartModelFactory shoppingCartModelFactory,
            IAbcCategoryService abcCategoryService,
            IProductService productService)
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _abcCategoryService = abcCategoryService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> Invoke(string widgetZone, object additionalData = null)
        {
            // Get the current shopping cart
            var cart = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync();
            
            // Check if any items in cart are appliances
            bool hasAppliances = false;
            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (await _abcCategoryService.HasApplianceTopLevelCategoryAsync(product))
                {
                    hasAppliances = true;
                    break;
                }
            }

            return View("~/Plugins/Widgets.CartSlideout/Views/Slideout.cshtml", hasAppliances);
        }
    }
}