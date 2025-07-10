using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Core;
using Nop.Web.Framework.Components;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Nop;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout.Components
{
    public class CartSlideoutViewComponent : NopViewComponent
    {
        private readonly IAbcCategoryService _abcCategoryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        public CartSlideoutViewComponent(
            IAbcCategoryService abcCategoryService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IWorkContext workContext)
        {
            _abcCategoryService = abcCategoryService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

            bool hasAppliance = false;

            foreach (var item in cart)
            {
                var productCategories = await _productService.GetProductCategoriesByProductIdAsync(item.ProductId);

                foreach (var productCategory in productCategories)
                {
                    if (await _abcCategoryService.HasApplianceTopLevelCategoryAsync(productCategory.CategoryId))
                    {
                        hasAppliance = true;
                        break;
                    }
                }

                if (hasAppliance)
                    break;
            }

            return View("~/Plugins/Widgets.CartSlideout/Views/Slideout.cshtml", hasAppliance);
        }
    }
}
