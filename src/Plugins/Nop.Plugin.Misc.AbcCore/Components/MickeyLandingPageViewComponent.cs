using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.AbcCore.Components
{
    /// <summary>
    /// Renders the currently active Mickey Shorr landing page product grid.
    /// Drop into any widget zone via: @await Component.InvokeAsync("MickeyLandingPage")
    /// </summary>
    [ViewComponent(Name = "MickeyLandingPage")]
    public class MickeyLandingPageViewComponent : NopViewComponent
    {
        private readonly IMickeyLandingPageService _landingPageService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;

        public MickeyLandingPageViewComponent(
            IMickeyLandingPageService landingPageService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreContext storeContext)
        {
            _landingPageService = landingPageService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var activeLandingPage = await _landingPageService.GetActiveLandingPageAsync();
            if (activeLandingPage == null)
                return Content(string.Empty);

            var products = await _landingPageService.GetProductsByLandingPageIdAsync(activeLandingPage.Id);

            // Filter to published, non-deleted products only
            var available = products
                .Where(p => p.Published && !p.Deleted)
                .ToList();

            if (!available.Any())
                return Content(string.Empty);

            var productModels = await _productModelFactory.PrepareProductOverviewModelsAsync(available);

            return View(
                "~/Plugins/Misc.AbcCore/Views/MickeyLandingPage/ProductList.cshtml",
                productModels.ToList());
        }
    }
}
