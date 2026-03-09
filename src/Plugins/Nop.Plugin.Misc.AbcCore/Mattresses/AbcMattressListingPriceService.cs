using Nop.Services.Catalog;
using System.Linq;
using System;
using Nop.Core;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.AbcCore.Mattresses
{
    public class AbcMattressListingPriceService : IAbcMattressListingPriceService
    {
        private readonly IAbcMattressEntryService _abcMattressEntryService;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IWebHelper _webHelper;

        public AbcMattressListingPriceService(
            IAbcMattressEntryService abcMattressEntryService,
            IProductService productService,
            IProductAttributeService productAttributeService,
            IWebHelper webHelper
        )
        {
            _abcMattressEntryService = abcMattressEntryService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _webHelper = webHelper;
        }

        public async Task<(decimal Price, decimal OldPrice)?> GetListingPriceForMattressProductAsync(int productId)
        {
            var url = _webHelper.GetThisPageUrl(true);
            var size = GetMattressSizeFromUrl(url);
            var mattressBase = GetBaseFromMattressSize(size);

            var pams = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            ProductAttributeMapping mattressSizePam = null;
            ProductAttributeMapping mattressBasePam = null;
            foreach (var pam in pams)
            {
                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId);
                if (productAttribute?.Name == AbcMattressesConsts.MattressSizeName)
                {
                    mattressSizePam = pam;
                }
                else if (productAttribute?.Name == mattressBase)
                {
                    mattressBasePam = pam;
                }
            }

            if (mattressSizePam == null) // if no mattress sizes, return price
            {
                return null;
            }

            var mattressSizeValue = (await _productAttributeService.GetProductAttributeValuesAsync(
                mattressSizePam.Id
            )).Where(pav => pav.Name == size)
             .FirstOrDefault();

            if (mattressSizeValue == null) // no matching sizes, check for default (queen)
            {
                mattressSizeValue = (await _productAttributeService.GetProductAttributeValuesAsync(
                    mattressSizePam.Id
                )).Where(pav => pav.Name == AbcMattressesConsts.Queen)
                .FirstOrDefault();
            }

            var mattressBaseValue = mattressBasePam == null ? null :
                (await _productAttributeService.GetProductAttributeValuesAsync(
                    mattressBasePam.Id
                )).OrderBy(pav => pav.DisplayOrder).FirstOrDefault();
            var mattressBasePriceAdjustment = mattressBaseValue?.PriceAdjustment ?? 0;

            var product = await _productService.GetProductByIdAsync(productId);

            return mattressSizeValue == null ?
                null :
                (Math.Round(product.Price + mattressSizeValue.PriceAdjustment + mattressBasePriceAdjustment, 2),
                mattressSizeValue.Cost + mattressBasePriceAdjustment); // using ProductAttributeValue Cost for OldPrice
        }

        private string GetBaseFromMattressSize(string size)
        {
            switch (size)
            {
                case AbcMattressesConsts.CaliforniaKing:
                    return AbcMattressesConsts.BaseNameCaliforniaKing;
                case AbcMattressesConsts.King:
                    return AbcMattressesConsts.BaseNameKing;
                case AbcMattressesConsts.Full:
                    return AbcMattressesConsts.BaseNameFull;
                case AbcMattressesConsts.TwinXL:
                    return AbcMattressesConsts.BaseNameTwinXL;
                case AbcMattressesConsts.Twin:
                    return AbcMattressesConsts.BaseNameTwin;
                default:
                    return AbcMattressesConsts.BaseNameQueen;
            }
        }

        // default to queen if nothing matches
        private string GetMattressSizeFromUrl(string url)
        {
            var slug = url.Substring(url.LastIndexOf('/') + 1);
            if (slug.Contains("california-king-mattress"))
            {
                return AbcMattressesConsts.CaliforniaKing;
            }
            if (slug.Contains("king-mattress"))
            {
                return AbcMattressesConsts.King;
            }
            if (slug.Contains("full-mattress"))
            {
                return AbcMattressesConsts.Full;
            }
            if (slug.Contains("twin-extra-long-mattress") ||
                slug.Contains("twinxl-mattress"))
            {
                return AbcMattressesConsts.TwinXL;
            }
            if (slug.Contains("twin-mattress"))
            {
                return AbcMattressesConsts.Twin;
            }

            return AbcMattressesConsts.Queen;
        }
    }
}
