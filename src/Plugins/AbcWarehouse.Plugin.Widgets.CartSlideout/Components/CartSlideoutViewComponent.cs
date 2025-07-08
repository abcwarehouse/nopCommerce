using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Nop;
using Nop.Services.ShoppingCart;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout.Components
{
    public class CartSlideoutViewComponent : NopViewComponent
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAbcCategoryService _abcCategoryService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        public CartSlideoutViewComponent(
            IShoppingCartService shoppingCartService,
            IAbcCategoryService abcCategoryService,
            IProductService productService,
            IWorkContext workContext)
        {
            _shoppingCartService = shoppingCartService;
            _abcCategoryService = abcCategoryService;
            _productService = productService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> Invoke(string widgetZone, object additionalData = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer);
            
            bool hasAppliances = false;
            foreach (var item in cart)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var productCategories = await _productService.GetProductCategoriesByProductIdAsync(product.Id);
                
                foreach (var pc in productCategories)
                {
                    if (await _abcCategoryService.HasApplianceTopLevelCategoryAsync(pc.CategoryId))
                    {
                        hasAppliances = true;
                        break;
                    }
                }
                if (hasAppliances) break;
            }

            return View("~/Plugins/Widgets.CartSlideout/Views/Slideout.cshtml", hasAppliances);
        }
    }
}