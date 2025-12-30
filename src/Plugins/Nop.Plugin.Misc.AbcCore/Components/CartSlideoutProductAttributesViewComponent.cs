using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Factories;
using Nop.Web.Framework.Components;
using Nop.Core.Domain.Orders;
using System.Linq;
using Nop.Plugin.Misc.AbcCore.Delivery;

namespace Nop.Plugin.Misc.AbcCore.Components
{
    public class CartSlideoutProductAttributesViewComponent : NopViewComponent
    {
        private readonly IAbcProductModelFactory _productModelFactory;

        public CartSlideoutProductAttributesViewComponent(
            IAbcProductModelFactory productModelFactory)
        {
            _productModelFactory = productModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            Product product,
            string[] includedAttributeNames,
            ShoppingCartItem updateCartItem = null)
        {
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product);

            model.ProductAttributes = await _productModelFactory.PrepareProductAttributeModelsAsync(
                product,
                updateCartItem,
                includedAttributeNames);

            var warrantyModel = model.ProductAttributes.FirstOrDefault(m => m.Name == AbcDeliveryConsts.WarrantyProductAttributeName);
            if (warrantyModel != null)
            {
                warrantyModel.Values = warrantyModel.Values.OrderBy(v => v.PriceAdjustmentValue).ToList();
            }

            return model != null ?
                View(model) :
                Content("");
        }
    }
}
