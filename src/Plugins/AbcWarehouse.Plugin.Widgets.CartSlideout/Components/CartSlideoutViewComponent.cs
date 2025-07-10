using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Nop;
using Nop.Services.ShoppingCart;
using Nop.Web.Models.ShoppingCart;
using Nop.Core.Domain.Orders;

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
            var model = new CartSlideoutModel
            {
                HasAppliances = await CheckForAppliancesInCart()
            };
            
            return View("~/Plugins/Widgets.CartSlideout/Views/Slideout.cshtml", model);
        }

        private async Task<bool> CheckForAppliancesInCart()
        {
            var cart = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync();
            
            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var productCategories = await _productService.GetProductCategoriesByProductIdAsync(product.Id);
                
                foreach (var pc in productCategories)
                {
                    if (await _abcCategoryService.HasApplianceTopLevelCategoryAsync(pc.CategoryId))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }

    public class CartSlideoutModel
    {
        public bool HasAppliances { get; set; }
    }
}