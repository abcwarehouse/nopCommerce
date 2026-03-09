using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.AbcCore.Extensions;
using Nop.Plugin.Misc.AbcCore.Models;

namespace Nop.Plugin.Misc.AbcCore.Components
{
    public class CartSlideoutProductInfoViewComponent : NopViewComponent
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAbcDescriptionService _productAbcDescriptionService;
        private readonly IProductService _productService;

        public CartSlideoutProductInfoViewComponent(
            IGenericAttributeService genericAttributeService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAbcDescriptionService productAbcDescriptionService,
            IProductService productService)
        {
            _genericAttributeService = genericAttributeService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAbcDescriptionService = productAbcDescriptionService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, int productId, decimal customerEnteredPrice)
        {
            if (productId <= 0)
            {
                throw new NopException($"Product ID is required. Provided value: {productId}");
            }

            var product = await _productService.GetProductByIdAsync(productId);
            var productName = product.Name;
            var productPicture = (await _productService.GetProductPicturesByProductIdAsync(product.Id)).FirstOrDefault();
            var pictureUrl = productPicture != null ?
                await _pictureService.GetPictureUrlAsync(productPicture.PictureId) :
                string.Empty;

            var shouldShowPrice = !await product.IsAddToCartToSeePriceAsync() &&
                                  !product.IsMattress();

            var model = new ProductInfoModel()
            {
                ImageUrl = pictureUrl,
                Name = productName,
                Description = await GetProductDescriptionAsync(product),
                OldPrice = shouldShowPrice && product.OldPrice != product.Price ?
                    await _priceFormatter.FormatPriceAsync(product.OldPrice + customerEnteredPrice) :
                    string.Empty,
                Price = shouldShowPrice ?
                    await _priceFormatter.FormatPriceAsync(product.Price + customerEnteredPrice) :
                    string.Empty,
            };

            return View("~/Plugins/Misc.AbcCore/Views/_ProductInfo.cshtml", model);
        }

        private async Task<string> GetProductDescriptionAsync(Product product)
        {
            var plpDescription = await _genericAttributeService.GetAttributeAsync<Product, string>(
                product.Id, "PLPDescription");

            if (plpDescription != null)
            {
                return plpDescription;
            }

            var pad = await _productAbcDescriptionService.GetProductAbcDescriptionByProductIdAsync(product.Id);
            return pad != null ? pad.AbcDescription : product.ShortDescription;
        }
    }
}
