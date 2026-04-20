using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Logging;
using Nop.Services.Messages;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Core.Domain.Security;
using Nop.Services.Customers;
using Nop.Services.Catalog;
using System.IO;
using Nop.Plugin.Misc.AbcCore;
using Nop.Web.Models.Catalog;
using System.Threading.Tasks;
using Nop.Services.Common;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Plugin.Misc.AbcCore.Models;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Web.Framework.Infrastructure;
using nopWebAdminCatalog = Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.AbcCore.Components
{
    [ViewComponent(Name = "AbcCore")]
    public class AbcCoreViewComponent : NopViewComponent
    {
        private readonly CoreSettings _coreSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly IMickeyLandingPageService _mickeyLandingPageService;

        public AbcCoreViewComponent(
            CoreSettings coreSettings,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            IMickeyLandingPageService mickeyLandingPageService
        ) {
            _coreSettings = coreSettings;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _mickeyLandingPageService = mickeyLandingPageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
            {
                var productId = (additionalData as ProductModel).Id;

                var plpDescription = await _genericAttributeService.GetAttributeAsync<Product, string>(
                    productId, "PLPDescription"
                );

                // Current landing page assignments for this product
                var currentAssignments = new List<MickeyLandingPageAssignment>();
                var availableLandingPages = new List<SelectListItem>();

                if (productId > 0)
                {
                    var allLandingPages = await _mickeyLandingPageService.GetAllLandingPagesAsync();
                    var mappings = await _mickeyLandingPageService.GetMappingsByProductIdAsync(productId);
                    var assignedIds = mappings.Select(m => m.MickeyLandingPageId).ToHashSet();

                    currentAssignments = mappings.Select(m =>
                    {
                        var lp = allLandingPages.FirstOrDefault(p => p.Id == m.MickeyLandingPageId);
                        return new MickeyLandingPageAssignment
                        {
                            MappingId = m.Id,
                            LandingPageId = m.MickeyLandingPageId,
                            Name = lp?.Name ?? "Unknown",
                            DateRange = lp?.GetDateRangeDisplay() ?? "",
                            IsActive = lp?.IsActive() ?? false
                        };
                    }).ToList();

                    availableLandingPages = allLandingPages
                        .Where(lp => !assignedIds.Contains(lp.Id))
                        .Select(lp => new SelectListItem
                        {
                            Value = lp.Id.ToString(),
                            Text = $"{lp.Name} ({lp.GetDateRangeDisplay()})"
                        }).ToList();
                }

                var model = new ABCProductDetailsModel
                {
                    ProductId = productId,
                    PLPDescription = plpDescription,
                    CurrentLandingPages = currentAssignments,
                    AvailableLandingPages = availableLandingPages
                };

                return View("~/Plugins/Misc.AbcCore/Views/ProductDetails.cshtml", model);
            }
            else if (widgetZone == AdminWidgetZones.HeaderBefore && !_coreSettings.IsValid())
            {
                _notificationService.ErrorNotification(
                    string.Format(
                        "Misc.AbcCore - Please <a href=\"{0}\">configure</a> the plugin to ensure working functionality.",
                        Url.Action("Configure", "AbcCore")),
                        false);
            }
            else if (widgetZone == PublicWidgetZones.Footer)
            {
                string sha = File.ReadAllText("Plugins/Misc.AbcCore/sha.txt").Trim();
                string branch = File.ReadAllText("Plugins/Misc.AbcCore/branch.txt").Trim();
                return View("~/Plugins/Misc.AbcCore/Views/BuildInfo.cshtml", (sha, branch));
            }
            else if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
            {
                var categoryId = (additionalData as nopWebAdminCatalog.CategoryModel).Id;
                var hawthornePictureId = await _genericAttributeService.GetAttributeAsync<Category, int>(
                    categoryId, "HawthornePictureId"
                );
                var hawthorneMetaTitle = await _genericAttributeService.GetAttributeAsync<Category, string>(
                    categoryId, "HawthorneMetaTitle"
                );
                var hawthorneMetaDescription = await _genericAttributeService.GetAttributeAsync<Category, string>(
                    categoryId, "HawthorneMetaDescription"
                );

                var model = new AbcCategoryDetailsModel
                {
                    CategoryId = categoryId,
                    HawthornePictureId = hawthornePictureId,
                    HawthorneMetaTitle = hawthorneMetaTitle,
                    HawthorneMetaDescription = hawthorneMetaDescription
                };

                return View("~/Plugins/Misc.AbcCore/Views/CategoryDetails.cshtml", model);
            }

            return Content("");
        }
    }
}
