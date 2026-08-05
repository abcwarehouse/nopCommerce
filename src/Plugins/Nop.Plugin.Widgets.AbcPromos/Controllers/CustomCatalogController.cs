using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Plugin.Misc.AbcPromos.Models;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Services.Seo;
using Nop.Services.Logging;
using Nop.Services.Catalog;
using System.Collections.Generic;
using Nop.Plugin.Widgets.AbcPromos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using System;
using Microsoft.AspNetCore.Http;
using Nop.Services.Stores;
using Nop.Plugin.Misc.AbcCore.Domain;

namespace Nop.Plugin.Misc.AbcPromos.Controllers
{
    public class CustomCatalogController : BasePublicController
    {
        private readonly IAbcPromoService _abcPromoService;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;

        private readonly IProductModelFactory _productModelFactory;
        private readonly ICatalogModelFactory _catalogModelFactory;

        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;

        private readonly AbcPromosSettings _settings;
        private readonly CatalogSettings _catalogSettings;

        public CustomCatalogController(
            IAbcPromoService abcPromoService,
            ICategoryService categoryService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            IProductModelFactory productModelFactory,
            ICatalogModelFactory categoryModelFactory,
            ILogger logger,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            AbcPromosSettings settings,
            CatalogSettings catalogSettings,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _abcPromoService = abcPromoService;
            _categoryService = categoryService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _productModelFactory = productModelFactory;
            _catalogModelFactory = categoryModelFactory;
            _logger = logger;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _settings = settings;
            _catalogSettings = catalogSettings;
        }

        public async Task<IActionResult> PromoListingPage()
        {
            var promos = _settings.IncludeExpiredPromosOnRebatesPromosPage ?
                            (await _abcPromoService.GetActivePromosAsync()).Union(await _abcPromoService.GetExpiredPromosAsync()) :
                            await _abcPromoService.GetActivePromosAsync();

            // filter by active store
            var activeStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var storePromos = new List<AbcPromo>();
            foreach (var promo in promos)
            {
                var storeMappings = await _storeMappingService.GetStoreMappingsAsync(promo);
                foreach (var sm in storeMappings)
                {
                    if (sm.StoreId == activeStoreId)
                    {
                        storePromos.Add(promo);
                        break;
                    }
                }
            }

            // Filter to promos with published products only
            storePromos = await storePromos.WhereAwait(async p => (await _abcPromoService.GetPublishedProductsByPromoIdAsync(p.Id)).Any()).ToListAsync();

            return View("~/Plugins/Widgets.AbcPromos/Views/PromoListingPage.cshtml", storePromos);
        }

        public async Task<IActionResult> Deals(CatalogProductsCommand command)
        {
            var activePromos = await _abcPromoService.GetActivePromosAsync();

            // filter by active store
            var activeStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var storePromos = new List<AbcPromo>();
            foreach (var promo in activePromos)
            {
                var storeMappings = await _storeMappingService.GetStoreMappingsAsync(promo);
                foreach (var sm in storeMappings)
                {
                    if (sm.StoreId == activeStoreId)
                    {
                        storePromos.Add(promo);
                        break;
                    }
                }
            }

            // Aggregate all products from all active promos
            var allDealsProducts = new Dictionary<int, Product>();
            var productPromoMap = new Dictionary<int, List<AbcPromo>>();

            foreach (var promo in storePromos)
            {
                var promoProducts = await _abcPromoService.GetPublishedProductsByPromoIdAsync(promo.Id);
                promoProducts = await FilterByStoreAsync(promoProducts);

                foreach (var product in promoProducts)
                {
                    if (!allDealsProducts.ContainsKey(product.Id))
                    {
                        allDealsProducts[product.Id] = product;
                        productPromoMap[product.Id] = new List<AbcPromo>();
                    }
                    productPromoMap[product.Id].Add(promo);
                }
            }

            var dealsProducts = allDealsProducts.Values.ToList();

            // Get unique parent categories from deals products
            var parentCategories = new Dictionary<int, Category>();
            foreach (var product in dealsProducts)
            {
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                foreach (var pc in productCategories)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
                    if (category != null && category.ParentCategoryId > 0)
                    {
                        var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
                        if (parentCategory != null && !parentCategories.ContainsKey(parentCategory.Id))
                        {
                            parentCategories[parentCategory.Id] = parentCategory;
                        }
                    }
                    else if (category != null && category.ParentCategoryId == 0)
                    {
                        if (!parentCategories.ContainsKey(category.Id))
                        {
                            parentCategories[category.Id] = category;
                        }
                    }
                }
            }

            // Apply promo filter
            var selectedPromoId = GetSelectedPromoFilterId();
            if (selectedPromoId.HasValue)
            {
                dealsProducts = dealsProducts
                    .Where(p => productPromoMap.ContainsKey(p.Id) && productPromoMap[p.Id].Any(promo => promo.Id == selectedPromoId.Value))
                    .ToList();
            }

            // Apply category filter
            var selectedCategoryId = GetSelectedCategoryFilterId();
            if (selectedCategoryId.HasValue)
            {
                var filteredProducts = new List<Product>();
                foreach (var product in dealsProducts)
                {
                    var pcs = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                    var categoryIds = pcs.Select(pc => pc.CategoryId).ToList();

                    // Check if product is in selected category or its children
                    var childCategoryIds = (await _categoryService.GetChildCategoryIdsAsync(selectedCategoryId.Value)).ToList();
                    childCategoryIds.Add(selectedCategoryId.Value);

                    if (categoryIds.Intersect(childCategoryIds).Any())
                    {
                        filteredProducts.Add(product);
                    }
                }
                dealsProducts = filteredProducts;
            }

            // Sort products
            dealsProducts = SortPromoProducts(dealsProducts, command);

            // Paginate
            var filteredDealsProducts = dealsProducts.Skip(command.PageIndex * 20).Take(20).ToList();

            // Group products by brand/manufacturer
            var productsByBrand = new Dictionary<string, List<Product>>();
            foreach (var product in filteredDealsProducts)
            {
                // Get the manufacturer from the promo(s) this product belongs to
                var manufacturerId = productPromoMap.ContainsKey(product.Id) && productPromoMap[product.Id].Any()
                    ? productPromoMap[product.Id].First().ManufacturerId
                    : null;

                var manufacturer = manufacturerId.HasValue
                    ? await _manufacturerService.GetManufacturerByIdAsync(manufacturerId.Value)
                    : null;

                var brandName = manufacturer?.Name ?? "Multiple Brands";

                if (!productsByBrand.ContainsKey(brandName))
                {
                    productsByBrand[brandName] = new List<Product>();
                }

                productsByBrand[brandName].Add(product);
            }

            var model = new DealsPageModel
            {
                Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(filteredDealsProducts)).ToList(),
                SelectedPromoId = selectedPromoId,
                SelectedCategoryId = selectedCategoryId
            };

            // Get selected promo banner and form URLs
            if (selectedPromoId.HasValue)
            {
                var selectedPromo = storePromos.FirstOrDefault(p => p.Id == selectedPromoId.Value);
                if (selectedPromo != null)
                {
                    model.SelectedPromoBannerUrl = await selectedPromo.GetPromoBannerUrlAsync();
                    model.SelectedPromoFormUrl = selectedPromo.GetPdfPath();
                }
            }

            // Populate ProductsByBrand with ProductOverviewModels
            foreach (var brandGroup in productsByBrand.OrderBy(g => g.Key))
            {
                var brandProducts = await _productModelFactory.PrepareProductOverviewModelsAsync(brandGroup.Value);
                model.ProductsByBrand[brandGroup.Key] = brandProducts.ToList();
            }

            var pagedList = new PagedList<Product>(
                filteredDealsProducts,
                command.PageIndex,
                20,
                dealsProducts.Count
            );
            model.LoadPagedList(pagedList);

            // Prepare sorting options
            await PrepareSortingOptionsAsync(model, command);

            // Prepare promo filters
            var promoSelectItems = new List<SelectListItem>
            {
                new SelectListItem { Text = "All Promos", Value = "", Selected = !selectedPromoId.HasValue }
            };
            foreach (var promo in storePromos.OrderBy(p => p.Name))
            {
                promoSelectItems.Add(new SelectListItem
                {
                    Text = promo.Description,
                    Value = promo.Id.ToString(),
                    Selected = selectedPromoId == promo.Id
                });
            }
            model.AvailablePromoFilters = promoSelectItems;

            // Prepare category filters (excluding brand and size related categories)
            var categorySelectItems = new List<SelectListItem>
            {
                new SelectListItem { Text = "All Categories", Value = "", Selected = !selectedCategoryId.HasValue }
            };
            foreach (var category in parentCategories.Values.OrderBy(c => c.Name))
            {
                var categoryNameLower = category.Name.ToLower();
                // Skip brand and size related categories
                if (categoryNameLower.Contains("brand") || categoryNameLower.Contains("size") ||
                    categoryNameLower.Contains("shop by brand") || categoryNameLower.Contains("shop by size"))
                {
                    continue;
                }

                categorySelectItems.Add(new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString(),
                    Selected = selectedCategoryId == category.Id
                });
            }
            model.AvailableCategoryFilters = categorySelectItems;

            return View("~/Plugins/Widgets.AbcPromos/Views/Deals.cshtml", model);
        }

        public async Task<IActionResult> Promo(string promoSlug, CatalogProductsCommand command)
        {
            // Set to high to low by default for gifts under
            if (promoSlug.Contains("gifts-under"))
            {
                command.OrderBy = 11;
            }

            var urlRecord = await _urlRecordService.GetBySlugAsync(promoSlug);
            if (urlRecord == null) return InvokeHttp404();

            var promo = await _abcPromoService.GetPromoByIdAsync(urlRecord.EntityId);
            if (promo == null) return InvokeHttp404();

            var shouldDisplay = _settings.IncludeExpiredPromosOnRebatesPromosPage ?
                promo.IsExpired() || promo.IsActive() :
                promo.IsActive();
            if (!shouldDisplay) return InvokeHttp404();

            var promoProducts = await _abcPromoService.GetPublishedProductsByPromoIdAsync(promo.Id);

            // need to filter by store here
            promoProducts = await FilterByStoreAsync(promoProducts);

            // if a category is provided, filter by it
            var filterCategory = await GetFilterCategoryAsync();
            if (filterCategory != null) {
                var filterCategoryIds = (await _categoryService.GetChildCategoryIdsAsync(filterCategory.Id)).ToList();
                filterCategoryIds.Add(filterCategory.Id);
                var categoryFilteredProducts = new List<Product>();
                foreach (var product in promoProducts)
                {
                    var pcs = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                    var pcsCategoryIds = pcs.Select(pc => pc.CategoryId);
                    var pcCategoryIdsWithParents = new List<int>();

                    if (pcsCategoryIds.Intersect(filterCategoryIds).Any())
                    {
                        categoryFilteredProducts.Add(product);
                    }
                }
                
                promoProducts = categoryFilteredProducts;
            }

            promoProducts = SortPromoProducts(promoProducts, command);

            var filteredPromoProducts = promoProducts.Skip(command.PageIndex * 20).Take(20).ToList();

            var model = new PromoListingModel
            {
                Name = promo.ManufacturerId != null ?
                            $"{(await _manufacturerService.GetManufacturerByIdAsync(promo.ManufacturerId.Value)).Name} - {promo.Description}" :
                            promo.Description,
                Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(filteredPromoProducts)).ToList(),
                BannerImageUrl = await promo.GetPromoBannerUrlAsync(),
                PromoFormPopup = promo.GetPdfPath()
            };

            var pagedList = new PagedList<Product>(
                filteredPromoProducts,
                command.PageIndex,
                20,
                promoProducts.Count
            );
            model.LoadPagedList(pagedList);

            // using duplicate sorting - it would be good to link this to NOP code but it's pretty complex
            await PrepareSortingOptionsAsync(model, command);

            return View("~/Plugins/Widgets.AbcPromos/Views/PromoListing.cshtml", model);
        }

        private async Task<List<Product>> FilterByStoreAsync(IList<Product> products)
        {
            var activeStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var result = new List<Product>();

            foreach (var product in products)
            {
                var storeMappings = await _storeMappingService.GetStoreMappingsAsync(product);
                if (storeMappings.Select(sm => sm.StoreId).Contains(activeStoreId))
                {
                    result.Add(product);
                }
            }

            return result;
        }

        private async Task<Category> GetFilterCategoryAsync()
        {
            var categorySlug = Request.Query["category"].FirstOrDefault();
            if (categorySlug == null) { return null; }

            var urlRecord = await _urlRecordService.GetBySlugAsync(categorySlug);
            if (urlRecord == null || urlRecord.EntityName != "Category") { return null; }

            return await _categoryService.GetCategoryByIdAsync(urlRecord.EntityId);
        }

        private int? GetSelectedPromoFilterId()
        {
            var promoIdStr = Request.Query["promo"].FirstOrDefault();
            if (string.IsNullOrEmpty(promoIdStr))
                return null;

            if (int.TryParse(promoIdStr, out var promoId))
                return promoId;

            return null;
        }

        private int? GetSelectedCategoryFilterId()
        {
            var categoryIdStr = Request.Query["category"].FirstOrDefault();
            if (string.IsNullOrEmpty(categoryIdStr))
                return null;

            if (int.TryParse(categoryIdStr, out var categoryId))
                return categoryId;

            return null;
        }

        private List<Product> SortPromoProducts(
            IList<Product> promoProducts,
            CatalogProductsCommand command
        )
        {
            if (command.OrderBy == 11)
            {
                return promoProducts.OrderByDescending(p => p.Price).ToList();
            }

            return promoProducts.OrderBy(p => p.Price).ToList();
        }

        private async Task PrepareSortingOptionsAsync(PromoListingModel model, CatalogProductsCommand command)
        {
            //set the order by position by default
            model.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();
            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            model.AllowProductSorting = true;
            command.OrderBy = model.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            foreach (var option in orderedActiveSortingOptions)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = option.Id.ToString(),
                    Selected = option.Id == command.OrderBy
                });
            }

            // Promo specific - only using price options
            model.AvailableSortOptions =
                model.AvailableSortOptions.Where(aso => aso.Text.Contains("Price:")).ToList();
        }

        private async Task PrepareSortingOptionsAsync(DealsPageModel model, CatalogProductsCommand command)
        {
            //set the order by position by default
            model.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();
            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            model.AllowProductSorting = true;
            command.OrderBy = model.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            foreach (var option in orderedActiveSortingOptions)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = option.Id.ToString(),
                    Selected = option.Id == command.OrderBy
                });
            }

            // Deals specific - only using price options
            model.AvailableSortOptions =
                model.AvailableSortOptions.Where(aso => aso.Text.Contains("Price:")).ToList();
        }
    }
}