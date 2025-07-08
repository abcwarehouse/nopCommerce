using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.AbcCore.Nop;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout.Components
{
    public class CartSlideoutViewComponent : NopViewComponent
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAbcCategoryService _abcCategoryService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        private readonly IProductCategoryService _productCategoryService;

        public CartSlideoutViewComponent(
            IShoppingCartService shoppingCartService,
            IAbcCategoryService abcCategoryService,
            IProductService productService,
             IProductCategoryService productCategoryService,
            IWorkContext workContext)
        {
            _shoppingCartService = shoppingCartService;
            _abcCategoryService = abcCategoryService;
            _productService = productService;
            _productCategoryService = productCategoryService;
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
                var productCategories = await _productCategoryService.GetProductCategoriesByProductIdAsync(product.Id);
                
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