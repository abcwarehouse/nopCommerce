using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public class SearchSpringService : ISearchSpringService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _searchUrl = "https://4lt84w.a.searchspring.io";
        private readonly string _personalizedUrl = "https://api.searchspring.io";
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SearchSpringService(IHttpClientFactory httpClientFactory, ILogger logger, IProductService productService, IProductModelFactory productModelFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null, string siteId = "4lt84w", int page = 1, Dictionary<string, List<string>> filters = null, string sort = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query must not be null or empty.", nameof(query));

            var queryParams = new List<string>
            {
                $"q={HttpUtility.UrlEncode(query)}",
                "resultsFormat=json",
                "resultsPerPage=25",
                $"page={page}",
                "redirectResponse=direct"
            };

            if (!string.IsNullOrEmpty(sessionId))
                queryParams.Add($"ss-sessionId={HttpUtility.UrlEncode(sessionId)}");

            if (!string.IsNullOrEmpty(siteId))
                queryParams.Add($"siteId={HttpUtility.UrlEncode(siteId)}");

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    foreach (var value in filter.Value)
                    {
                        queryParams.Add($"filter.{HttpUtility.UrlEncode(filter.Key)}={HttpUtility.UrlEncode(value)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var parts = sort.Contains(":") ? sort.Split(':') :
                            sort.Contains("_") ? sort.Split('_') : null;

                if (parts?.Length == 2)
                {
                    var field = parts[0];
                    var direction = parts[1];
                    queryParams.Add($"sort.{HttpUtility.UrlEncode(field)}={HttpUtility.UrlEncode(direction)}");
                }
                else
                {
                    queryParams.Add("sort.relevance=desc");
                }
            }
            else
            {
                queryParams.Add("sort.relevance=desc");
            }

            var url = $"{_searchUrl}/api/search/search.json?{string.Join("&", queryParams)}";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400 && response.Headers.Location != null)
            {
                return new SearchResultModel { RedirectResponse = response.Headers.Location.ToString() };
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Searchspring returned error {response.StatusCode}: {json}\nURL: {url}");

            try
            {
                var productList = new List<SearchSpringProductModel>();
                int currentPage = 1, pageSize = 25, totalResults = 0;
                var facets = new Dictionary<string, FacetDetail>();
                var sortOptions = new List<SortOption>();
                var bannersByPosition = new Dictionary<string, List<string>>();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("redirect", out var redirectProp) &&
                    redirectProp.TryGetProperty("url", out var redirectUrlProp))
                {
                    return new SearchResultModel { RedirectResponse = redirectUrlProp.GetString() };
                }

                if (root.TryGetProperty("results", out var resultsElement))
                {
                    foreach (var item in resultsElement.EnumerateArray())
                    {
                        productList.Add(new SearchSpringProductModel
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            ProductUrl = item.GetProperty("url").GetString(),
                            ImageUrl = item.GetProperty("imageUrl").GetString(),
                            Price = item.GetProperty("price").GetString(),
                            Brand = item.GetProperty("brand").GetString(),
                            Category = item.GetProperty("category").GetString(),
                            ItemNumber = item.GetProperty("item_number").GetString(),
                            RetailPrice = item.GetProperty("retail_price").GetString(),
                            Sku = item.GetProperty("sku").GetString()
                        });
                    }
                }

                return new SearchResultModel
                {
                    Results = productList,
                    PageNumber = currentPage,
                    PageSize = pageSize,
                    TotalResults = totalResults,
                    Facets = facets,
                    SortOptions = sortOptions,
                    BannersByPosition = bannersByPosition
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse Searchspring response.", ex);
            }
        }

        public async Task<List<SearchSpringProductModel>> GetPersonalizedResultsAsync(string userId, string sessionId, string siteId = "4lt84w")
        {
            var client = _httpClientFactory.CreateClient();

            var queryParams = new List<string>
            {
                $"siteId={HttpUtility.UrlEncode(siteId)}",
                $"userId={HttpUtility.UrlEncode(userId)}",
                $"sessionId={HttpUtility.UrlEncode(sessionId)}"
            };

            var url = $"{_personalizedUrl}/api/personalized.json?{string.Join("&", queryParams)}";
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _logger.WarningAsync($"[SearchSpring] Personalization error: {response.StatusCode} - {json}");
                return new List<SearchSpringProductModel>();
            }

            var results = new List<SearchSpringProductModel>();

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("results", out var resultsElement))
                {
                    foreach (var item in resultsElement.EnumerateArray())
                    {
                        results.Add(new SearchSpringProductModel
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            ProductUrl = item.GetProperty("url").GetString(),
                            ImageUrl = item.GetProperty("imageUrl").GetString(),
                            Price = item.GetProperty("price").GetString(),
                            Brand = item.GetProperty("brand").GetString(),
                            Category = item.GetProperty("category").GetString(),
                            ItemNumber = item.GetProperty("item_number").GetString(),
                            RetailPrice = item.GetProperty("retail_price").GetString(),
                            Sku = item.GetProperty("sku").GetString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"[SearchSpring] Failed to parse personalization response: {ex.Message}", ex);
            }

            return results;
        }

        public string GetSearchSpringShopperId()
        {
            var cookieName = "ssShopperId";
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null && request.Cookies.TryGetValue(cookieName, out var shopperId) && !string.IsNullOrWhiteSpace(shopperId))
                return shopperId;

            return Guid.NewGuid().ToString();
        }

        public string GetSearchSpringSessionId()
        {
            var context = _httpContextAccessor?.HttpContext;
            var cookie = context?.Request?.Cookies["ssSessionId"];
            return cookie ?? Guid.NewGuid().ToString();
        }

        public async Task<Product> FindProductBySkuOrAltSkuAsync(string sku)
        {
            var products = await _productService.SearchProductsAsync(keywords: sku, searchDescriptions: true);
            return products.FirstOrDefault();
        }

        public async Task<IList<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products)
        {
            return (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
        }
    }
}
