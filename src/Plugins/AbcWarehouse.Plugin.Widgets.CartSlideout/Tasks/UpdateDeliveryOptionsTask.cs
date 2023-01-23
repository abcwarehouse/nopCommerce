using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.AbcCore;
using Nop.Plugin.Misc.AbcCore.Delivery;
using Nop.Plugin.Misc.AbcCore.Nop;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.AbcCore.Mattresses;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout.Tasks
{
    public partial class UpdateDeliveryOptionsTask : IScheduleTask
    {
        private readonly CoreSettings _coreSettings;
        private readonly IAbcDeliveryService _abcDeliveryService;
        private readonly IAbcMattressModelService _abcMattressModelService;
        private readonly IAbcProductAttributeService _abcProductAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger _logger;
        private readonly IPriceFormatter _priceFormatter;

        private ProductAttribute _deliveryPickupOptionsProductAttribute;
        private ProductAttribute _haulAwayDeliveryProductAttribute;
        private ProductAttribute _haulAwayDeliveryInstallProductAttribute;
        private ProductAttribute _pickupInStoreProductAttribute;

        public UpdateDeliveryOptionsTask(
            CoreSettings coreSettings,
            IAbcDeliveryService abcDeliveryService,
            IAbcMattressModelService abcMattressModelService,
            IAbcProductAttributeService abcProductAttributeService,
            ICategoryService categoryService,
            ILogger logger,
            IPriceFormatter priceFormatter)
        {
            _coreSettings = coreSettings;
            _abcDeliveryService = abcDeliveryService;
            _abcMattressModelService = abcMattressModelService;
            _abcProductAttributeService = abcProductAttributeService;
            _categoryService = categoryService;
            _logger = logger;
            _priceFormatter = priceFormatter;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            _deliveryPickupOptionsProductAttribute =
                await _abcProductAttributeService.GetProductAttributeByNameAsync(AbcDeliveryConsts.DeliveryPickupOptionsProductAttributeName);
            _haulAwayDeliveryProductAttribute =
                await _abcProductAttributeService.GetProductAttributeByNameAsync(AbcDeliveryConsts.HaulAwayDeliveryProductAttributeName);
            _haulAwayDeliveryInstallProductAttribute =
                await _abcProductAttributeService.GetProductAttributeByNameAsync(AbcDeliveryConsts.HaulAwayDeliveryInstallProductAttributeName);
            _pickupInStoreProductAttribute =
                await _abcProductAttributeService.GetProductAttributeByNameAsync(AbcDeliveryConsts.LegacyPickupInStoreProductAttributeName);

            var hasErrors = false;

            var abcDeliveryMaps = await _abcDeliveryService.GetAbcDeliveryMapsAsync();
            foreach (var map in abcDeliveryMaps)
            {
                var productIds = (await _categoryService.GetProductCategoriesByCategoryIdAsync(map.CategoryId)).Select(pc => pc.ProductId);
                foreach (var productId in productIds)
                {
                    try
                    {
                        var deliveryOptionsPam = await UpdateDeliveryOptionsPamAsync(productId, map);
                        if (deliveryOptionsPam != null)
                        {
                            var deliveryOptionPavs = await UpdateDeliveryOptionsPavAsync(deliveryOptionsPam, map);

                            // Try running these synchronously to prevent the PAV collision issues
                            var deliveryOnlyPav = deliveryOptionPavs.SingleOrDefault(pav => pav.Name.Contains("Home Delivery ("));
                            if (deliveryOnlyPav != null)
                            {
                                UpdateHaulawayAsync(productId, map.DeliveryHaulway, _haulAwayDeliveryProductAttribute.Id, deliveryOptionsPam.Id, deliveryOnlyPav);
                            }

                            var deliveryInstallPav = deliveryOptionPavs.SingleOrDefault(pav => pav.Name.Contains("Home Delivery and Installation ("));
                            if (deliveryInstallPav != null)
                            {
                                UpdateHaulawayAsync(productId, map.DeliveryHaulwayInstall, _haulAwayDeliveryInstallProductAttribute.Id, deliveryOptionsPam.Id, deliveryInstallPav);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        await _logger.InsertLogAsync(
                            LogLevel.Error,
                            $"Failure when updating delivery options for CategoryId {map.CategoryId}, ProductId {productId}",
                            e.ToString());
                        hasErrors = true;
                    }
                }
            }

            // Mattresses
            var mattressProductIds = _abcMattressModelService.GetAllAbcMattressModels()
                                                             .Where(amm => amm.ProductId != null)
                                                             .Select(amm => (int)amm.ProductId);
            foreach (var mattressProductId in mattressProductIds)
            {
                try
                {
                    // Hardcoding 2 as the "Mattress Delivery Only" option
                    var mattressAbcDeliveryMap = new AbcDeliveryMap() { DeliveryOnly = 2 };
                    var deliveryOptionsPam = await UpdateDeliveryOptionsPamAsync(mattressProductId, mattressAbcDeliveryMap);
                    await UpdateDeliveryOptionsPavAsync(deliveryOptionsPam, mattressAbcDeliveryMap);
                }
                catch (Exception e)
                {
                    await _logger.InsertLogAsync(
                        LogLevel.Error,
                        $"Failure when updating delivery options for Mattress ProductId {mattressProductId}",
                        e.ToString());
                    hasErrors = true;
                }
            }

            if (hasErrors)
            {
                throw new NopException("Failures occured when updating product delivery options.");
            }
        }

        private async System.Threading.Tasks.Task UpdateHaulawayAsync(
            int productId,
            int mapItemNumber,
            int haulAwayProductAttributeId,
            int deliveryOptionsPamId,
            ProductAttributeValue deliveryPav)
        {
            var existingPam =
                (await _abcProductAttributeService.GetProductAttributeMappingsByProductIdAsync(productId)).FirstOrDefault(
                    pam => pam.ProductAttributeId == haulAwayProductAttributeId
                );
            var newPam = mapItemNumber != 0 ? new ProductAttributeMapping()
            {
                ProductId = productId,
                ProductAttributeId = haulAwayProductAttributeId,
                AttributeControlType = AttributeControlType.Checkboxes,
                TextPrompt = "Haul Away",
                ConditionAttributeXml = $"<Attributes><ProductAttribute ID=\"{deliveryOptionsPamId}\"><ProductAttributeValue><Value>{deliveryPav.Id}</Value></ProductAttributeValue></ProductAttribute></Attributes>",
            } : null;

            var resultPam = await SaveProductAttributeMappingAsync(existingPam, newPam);

            if (resultPam != null)
            {
                var existingPav = (await _abcProductAttributeService.GetProductAttributeValuesAsync(resultPam.Id)).FirstOrDefault();

                var item = await _abcDeliveryService.GetAbcDeliveryItemByItemNumberAsync(mapItemNumber);
                var price = item.Price - deliveryPav.PriceAdjustment;
                var priceFormatted = price == 0 ? "FREE" : await _priceFormatter.FormatPriceAsync(price);
                var newPav = new ProductAttributeValue()
                {
                    ProductAttributeMappingId = resultPam.Id,
                    Name = string.Format("Remove Old Appliance ({0})", priceFormatted),
                    Cost = mapItemNumber,
                    PriceAdjustment = price,
                    IsPreSelected = false,
                    DisplayOrder = 0,
                };

                await SaveProductAttributeValueAsync(existingPav, newPav);
            }
        }

        private async System.Threading.Tasks.Task<ProductAttributeMapping> UpdateDeliveryOptionsPamAsync(
            int productId,
            AbcDeliveryMap map)
        {
            var existingPam =
                (await _abcProductAttributeService.GetProductAttributeMappingsByProductIdAsync(productId)).FirstOrDefault(
                    pam => pam.ProductAttributeId == _deliveryPickupOptionsProductAttribute.Id
                );
            // Only create if either delivery or delivery/install enabled for product
            var newPam = map.DeliveryOnly != 0 || map.DeliveryInstall != 0 ? new ProductAttributeMapping()
            {
                ProductId = productId,
                ProductAttributeId = _deliveryPickupOptionsProductAttribute.Id,
                AttributeControlType = AttributeControlType.RadioList,
            } : null;

            return await SaveProductAttributeMappingAsync(existingPam, newPam);
        }

        private async System.Threading.Tasks.Task<List<ProductAttributeValue>> UpdateDeliveryOptionsPavAsync(
            ProductAttributeMapping deliveryOptionsPam,
            AbcDeliveryMap map
        )
        {
            var existingValues = await _abcProductAttributeService.GetProductAttributeValuesAsync(deliveryOptionsPam.Id);
            var results = new List<ProductAttributeValue>();

            // TODO: This could be refactored to clean up the repeating
            // Delivery only
            var deliveryOnlyItem = map.DeliveryOnly == 0 ? new AbcDeliveryItem() : await _abcDeliveryService.GetAbcDeliveryItemByItemNumberAsync(map.DeliveryOnly);
            var deliveryOnlyPriceFormatted = map.DeliveryOnly == 2 ?
                "MATTRESS" : 
                await _priceFormatter.FormatPriceAsync(deliveryOnlyItem.Price);

            // Need to get the category to determine if furniture
            string message = await GetHomeDeliveryMessageAsync(
                deliveryOnlyPriceFormatted,
                deliveryOptionsPam.ProductId);

            var existingDeliveryOnlyPav = existingValues.Where(pav => pav.Name.Contains("Home Delivery (")).SingleOrDefault();
            var newDeliveryOnlyPav = map.DeliveryOnly != 0 ? new ProductAttributeValue()
            {
                ProductAttributeMappingId = deliveryOptionsPam.Id,
                Name = message,
                Cost = map.DeliveryOnly,
                PriceAdjustment = deliveryOnlyItem.Price,
                IsPreSelected = true,
                DisplayOrder = 10,
            } : null;

            var resultDeliveryOnlyPav = await SaveProductAttributeValueAsync(existingDeliveryOnlyPav, newDeliveryOnlyPav);
            if (resultDeliveryOnlyPav != null) { results.Add(resultDeliveryOnlyPav); }

            // Delivery/Install
            var deliveryInstallItem = map.DeliveryInstall == 0 ? new AbcDeliveryItem() : await _abcDeliveryService.GetAbcDeliveryItemByItemNumberAsync(map.DeliveryInstall);
            var deliveryInstallPriceFormatted = await _priceFormatter.FormatPriceAsync(deliveryInstallItem.Price);

            var existingDeliveryInstallPav = existingValues.Where(pav => pav.Name.Contains("Home Delivery and Installation (")).SingleOrDefault();
            var newDeliveryInstallPav = map.DeliveryInstall != 0 ? new ProductAttributeValue()
            {
                ProductAttributeMappingId = deliveryOptionsPam.Id,
                Name = string.Format("Home Delivery and Installation ({0})", deliveryInstallPriceFormatted),
                Cost = map.DeliveryInstall,
                PriceAdjustment = deliveryInstallItem.Price,
                IsPreSelected = false,
                DisplayOrder = 20,
            } : null;

            var resultDeliveryInstallPav = await SaveProductAttributeValueAsync(existingDeliveryInstallPav, newDeliveryInstallPav);
            if (resultDeliveryInstallPav != null) { results.Add(resultDeliveryInstallPav); }

            // Pickup - does not need to be returned
            var existingPickupPav = existingValues.Where(pav => pav.Name.Contains("Pickup In-Store")).SingleOrDefault();
            ProductAttributeValue newPickupPav = null;
            var pams = await _abcProductAttributeService.GetProductAttributeMappingsByProductIdAsync(deliveryOptionsPam.ProductId);
            var legacyPickupPam = await pams.WhereAwait(
                async pam => (await _abcProductAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId)).Name ==
                              AbcDeliveryConsts.LegacyPickupInStoreProductAttributeName).FirstOrDefaultAsync();
            if (legacyPickupPam != null)
            {
                newPickupPav = new ProductAttributeValue()
                {
                    ProductAttributeMappingId = deliveryOptionsPam.Id,
                    Name = "Pickup In-Store Or Curbside (FREE)",
                    // Used as the current placeholder for pickup in store
                    Cost = 1,
                    PriceAdjustment = 0,
                    IsPreSelected = false,
                    DisplayOrder = 0,
                };
            }
            await SaveProductAttributeValueAsync(existingPickupPav, newPickupPav);

            return results;
        }

        private bool DoProductAttributeValuesMatch(ProductAttributeValue existingPav, ProductAttributeValue newPav)
        {
            return existingPav.Name == newPav.Name;
        }

        private async System.Threading.Tasks.Task<ProductAttributeMapping> SaveProductAttributeMappingAsync(ProductAttributeMapping existingPam, ProductAttributeMapping newPam)
        {
            if (existingPam == null && newPam != null)
            {
                await _abcProductAttributeService.InsertProductAttributeMappingAsync(newPam);
                return newPam;
            }
            else if (existingPam != null && newPam == null)
            {
                await _abcProductAttributeService.DeleteProductAttributeMappingAsync(existingPam);
                return null;
            }
            
            return existingPam;
        }

        // Try making these synchronous to prevent collision issues
        private async System.Threading.Tasks.Task<ProductAttributeValue> SaveProductAttributeValueAsync(ProductAttributeValue existingPav, ProductAttributeValue newPav)
        {
            if (existingPav == null && newPav == null)
            {
                return null;
            }
            else if (existingPav == null && newPav != null)
            {
                await _abcProductAttributeService.InsertProductAttributeValueAsync(newPav);
                return newPav;
            }
            else if (existingPav != null && newPav == null)
            {
                await _abcProductAttributeService.DeleteProductAttributeValueAsync(existingPav);
                return null;
            }
            else if (!ArePavsEqual(existingPav, newPav))
            {
                existingPav.Name = newPav.Name;
                existingPav.PriceAdjustment = newPav.PriceAdjustment;
                existingPav.Cost = newPav.Cost;
                await _abcProductAttributeService.UpdateProductAttributeValueAsync(existingPav);
                return existingPav;
            }

            return existingPav;
        }

        private bool ArePavsEqual(ProductAttributeValue existingPav, ProductAttributeValue newPav)
        {
            return existingPav.Name == newPav.Name
                && existingPav.PriceAdjustment == newPav.PriceAdjustment
                && existingPav.Cost == newPav.Cost;
        }

        private async System.Threading.Tasks.Task<string> GetHomeDeliveryMessageAsync(string deliveryOnlyPriceFormatted, int productId)
        {
            if (deliveryOnlyPriceFormatted == "MATTRESS")
            {
                return "Home Delivery (Price in Cart)";
            }

            // If Furniture, no mail-in rebate
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            foreach (var pc in productCategories)
            {
                var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
                var fullCategoryListNames = (await _categoryService.GetCategoryBreadCrumbAsync(category)).Select(c => c.Name);
                if (fullCategoryListNames.Contains("Furniture"))
                {
                    return string.Format("Home Delivery ({0})", deliveryOnlyPriceFormatted);
                }
            }

            return string.Format("Home Delivery ({0}, FREE With Mail-In Rebate)", deliveryOnlyPriceFormatted);
        }
    }
}