using System.Collections.Generic;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public interface ISearchSpringService
    {
        Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null, string siteId = "4lt84w", int page = 1, Dictionary<string, List<string>> filters = null, string sort = null);
        Task<List<SearchSpringProductModel>> GetPersonalizedResultsAsync(string userId, string sessionId, string siteId = "4lt84w");
        Task<Product> FindProductBySkuOrAltSkuAsync(string sku);
        Task<IList<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products);
        string GetSearchSpringShopperId();
        public string GetSearchSpringSessionId();
    }
}