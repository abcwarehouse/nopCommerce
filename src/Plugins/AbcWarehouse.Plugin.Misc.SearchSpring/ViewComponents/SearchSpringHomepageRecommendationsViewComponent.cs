using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Core;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Components
{
    [ViewComponent(Name = "SearchSpringHomepageRecommendations")]
    public class SearchSpringHomepageRecommendationsViewComponent : Nop.Web.Framework.Components.NopViewComponent
    {
        private readonly ISearchSpringService _searchSpringService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkContext _workContext;

        public SearchSpringHomepageRecommendationsViewComponent(
            ISearchSpringService searchSpringService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWorkContext workContext)
        {
            _searchSpringService = searchSpringService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var customer = await _workContext.GetCurrentCustomerAsync();
            var shopperId = customer?.Id.ToString();

            var recsRequest = new RecommendationsRequestModel
            {
                Tags = "home",
                Shopper = shopperId,
                Limits = "12,8",
                SiteId = "4lt84w"
            };

            var recommendations = await _searchSpringService.GetRecommendationsAsync(recsRequest);

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Shared/Components/SearchSpring/_HomepageRecommendations.cshtml", recommendations);
        }
    }
}
