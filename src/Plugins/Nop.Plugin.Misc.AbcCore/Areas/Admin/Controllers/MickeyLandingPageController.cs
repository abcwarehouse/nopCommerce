using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbcCore.Areas.Admin.Models;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class MickeyLandingPageController : BaseAdminController
    {
        private readonly IMickeyLandingPageService _landingPageService;
        private readonly IProductService _productService;

        public MickeyLandingPageController(
            IMickeyLandingPageService landingPageService,
            IProductService productService)
        {
            _landingPageService = landingPageService;
            _productService = productService;
        }

        // ── List ────────────────────────────────────────────────────────────────

        public IActionResult List()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/List.cshtml",
                new MickeyLandingPageSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> List(MickeyLandingPageSearchModel searchModel)
        {
            var landingPages = (await _landingPageService.GetAllLandingPagesAsync())
                .ToPagedList(searchModel);

            var model = new MickeyLandingPageListModel().PrepareToGrid(searchModel, landingPages, () =>
                landingPages.Select(lp =>
                {
                    var mappings = _landingPageService.GetMappingsByLandingPageIdAsync(lp.Id)
                        .GetAwaiter().GetResult();
                    return new MickeyLandingPageModel
                    {
                        Id = lp.Id,
                        Name = lp.Name,
                        StartDate = lp.StartDate,
                        EndDate = lp.EndDate,
                        IsActive = lp.IsActive(),
                        DateRange = lp.GetDateRangeDisplay(),
                        ProductCount = mappings.Count
                    };
                }));

            return Json(model);
        }

        // ── Create ──────────────────────────────────────────────────────────────

        public IActionResult Create()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/Edit.cshtml",
                new MickeyLandingPageModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(MickeyLandingPageModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/Edit.cshtml", model);

            var landingPage = new MickeyLandingPage
            {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            await _landingPageService.InsertLandingPageAsync(landingPage);
            return RedirectToAction(nameof(Edit), new { id = landingPage.Id });
        }

        // ── Edit ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(int id)
        {
            var landingPage = await _landingPageService.GetLandingPageByIdAsync(id);
            if (landingPage == null)
                return RedirectToAction(nameof(List));

            var mappings = await _landingPageService.GetMappingsByLandingPageIdAsync(id);
            var model = new MickeyLandingPageModel
            {
                Id = landingPage.Id,
                Name = landingPage.Name,
                StartDate = landingPage.StartDate,
                EndDate = landingPage.EndDate,
                IsActive = landingPage.IsActive(),
                DateRange = landingPage.GetDateRangeDisplay(),
                ProductCount = mappings.Count
            };

            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/Edit.cshtml",
                model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MickeyLandingPageModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/Edit.cshtml", model);

            var landingPage = await _landingPageService.GetLandingPageByIdAsync(model.Id);
            if (landingPage == null)
                return RedirectToAction(nameof(List));

            landingPage.Name = model.Name;
            landingPage.StartDate = model.StartDate;
            landingPage.EndDate = model.EndDate;
            await _landingPageService.UpdateLandingPageAsync(landingPage);

            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/MickeyLandingPage/Edit.cshtml",
                model);
        }

        // ── Delete ──────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var landingPage = await _landingPageService.GetLandingPageByIdAsync(id);
            if (landingPage != null)
                await _landingPageService.DeleteLandingPageAsync(landingPage);

            return RedirectToAction(nameof(List));
        }

        // ── Product grid for a landing page ─────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> ProductList(MickeyLandingPageProductSearchModel searchModel)
        {
            var mappings = await _landingPageService.GetMappingsByLandingPageIdAsync(searchModel.LandingPageId);
            var pagedMappings = mappings.ToPagedList(searchModel);

            var model = await new MickeyLandingPageProductListModel()
                .PrepareToGridAsync(searchModel, pagedMappings, async () =>
                {
                    var results = new System.Collections.Generic.List<MickeyLandingPageProductModel>();
                    foreach (var mapping in pagedMappings)
                    {
                        var product = await _productService.GetProductByIdAsync(mapping.ProductId);
                        if (product == null) continue;
                        results.Add(new MickeyLandingPageProductModel
                        {
                            Id = mapping.Id,
                            MappingId = mapping.Id,
                            ProductId = product.Id,
                            Name = product.Name,
                            Sku = product.Sku,
                            Published = product.Published,
                            DisplayOrder = mapping.DisplayOrder
                        });
                    }
                    return results;
                });

            return Json(model);
        }

        // ── AJAX: Add product to landing page ───────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> AddProduct(int landingPageId, int productId)
        {
            var landingPage = await _landingPageService.GetLandingPageByIdAsync(landingPageId);
            if (landingPage == null)
                return Json(new { success = false, message = "Landing page not found." });

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            // Prevent duplicate mappings
            var existing = await _landingPageService.GetMappingsByLandingPageIdAsync(landingPageId);
            if (existing.Any(m => m.ProductId == productId))
                return Json(new { success = false, message = "Product is already on this landing page." });

            await _landingPageService.InsertMappingAsync(new MickeyLandingPageProductMapping
            {
                MickeyLandingPageId = landingPageId,
                ProductId = productId,
                DisplayOrder = 0
            });

            return Json(new { success = true });
        }

        // ── AJAX: Remove product from landing page ──────────────────────────────

        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int mappingId)
        {
            var mapping = await _landingPageService.GetMappingByIdAsync(mappingId);
            if (mapping == null)
                return Json(new { success = false, message = "Mapping not found." });

            await _landingPageService.DeleteMappingAsync(mapping);
            return Json(new { success = true });
        }

        // ── AJAX: Get landing pages for a product (used by product edit page) ───

        [HttpGet]
        public async Task<IActionResult> GetProductLandingPages(int productId)
        {
            var mappings = await _landingPageService.GetMappingsByProductIdAsync(productId);
            var allPages = await _landingPageService.GetAllLandingPagesAsync();

            var assignments = mappings.Select(m =>
            {
                var lp = allPages.FirstOrDefault(p => p.Id == m.MickeyLandingPageId);
                return new
                {
                    mappingId = m.Id,
                    landingPageId = m.MickeyLandingPageId,
                    name = lp?.Name ?? "Unknown",
                    dateRange = lp?.GetDateRangeDisplay() ?? "",
                    isActive = lp?.IsActive() ?? false
                };
            }).ToList();

            return Json(new { success = true, assignments });
        }
    }
}
