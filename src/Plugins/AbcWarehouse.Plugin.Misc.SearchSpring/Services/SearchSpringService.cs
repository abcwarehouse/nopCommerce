using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public class SearchSpringService : ISearchSpringService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BaseUrl = "https://4lt84w.a.searchspring.io";

        public SearchSpringService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null, string siteId = "4lt84w", int page = 1, Dictionary<string, List<string>> filters = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query must not be null or empty.", nameof(query));

            var client = _httpClientFactory.CreateClient();
            var queryParams = new List<string>
            {
                $"q={WebUtility.UrlEncode(query)}",
                "resultsFormat=json",
                "resultsPerPage=24",
                $"page={page}",
                "redirectResponse=minimal",
                $"siteId={WebUtility.UrlEncode(siteId)}"
            };

            if (!string.IsNullOrEmpty(sessionId))
                queryParams.Add($"ss-sessionId={WebUtility.UrlEncode(sessionId)}");

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    foreach (var value in filter.Value)
                    {
                        queryParams.Add($"filter[{WebUtility.UrlEncode(filter.Key)}]={WebUtility.UrlEncode(value)}");
                    }
                }
            }

            var url = $"{BaseUrl}/api/search/search.json?{string.Join("&", queryParams)}";
            Console.WriteLine($"[SearchSpring] Requesting URL: {url}");

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[SearchSpring] Response ({(int)response.StatusCode}): {json}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Searchspring returned error {response.StatusCode}: {json}\nURL: {url}");

            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var productList = ParseProducts(root);
                var pagination = ParsePagination(root);
                var facets = ParseFacets(root);
                var sortOptions = ParseSortOptions(root);

                return new SearchResultModel
                {
                    Results = productList,
                    PageNumber = pagination.pageNumber,
                    PageSize = pagination.pageSize,
                    TotalResults = pagination.totalResults,
                    Facets = facets,
                    SortOptions = sortOptions
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SearchSpring] JSON Deserialization failed: {ex.Message}");
                throw new Exception("Failed to parse Searchspring response.", ex);
            }
        }

        private List<SearchSpringProductModel> ParseProducts(JsonElement root)
        {
            var products = new List<SearchSpringProductModel>();

            if (root.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in resultsElement.EnumerateArray())
                {
                    var model = new SearchSpringProductModel
                    {
                        Id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : "",
                        Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "",
                        ProductUrl = item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : "",
                        ImageUrl = item.TryGetProperty("imageUrl", out var imgProp) ? imgProp.GetString() : "",
                        Price = item.TryGetProperty("price", out var priceProp) ? priceProp.GetString() : "",
                        Brand = item.TryGetProperty("brand", out var brandProp) ? brandProp.GetString() : "",
                        Category = item.TryGetProperty("category", out var catProp) ? catProp.GetString() : "",
                        ItemNumber = item.TryGetProperty("item_number", out var itemNumProp) ? itemNumProp.GetString() : "",
                        RetailPrice = item.TryGetProperty("retail_price", out var retailPriceProp) ? retailPriceProp.GetString() : "",
                        Sku = item.TryGetProperty("sku", out var skuProp) ? skuProp.GetString() : ""
                    };

                    products.Add(model);
                }
            }

            return products;
        }


        private (int pageNumber, int pageSize, int totalResults) ParsePagination(JsonElement root)
        {
            int pageNumber = 1, pageSize = 24, totalResults = 0;

            if (root.TryGetProperty("pagination", out var pagination) && pagination.ValueKind == JsonValueKind.Object)
            {
                pageNumber = pagination.GetProperty("currentPage").GetInt32();
                pageSize = pagination.GetProperty("pageSize").GetInt32();
                totalResults = pagination.GetProperty("totalResults").GetInt32();
            }

            return (pageNumber, pageSize, totalResults);
        }

        private Dictionary<string, FacetDetail> ParseFacets(JsonElement root)
        {
            var facets = new Dictionary<string, FacetDetail>();

            if (root.TryGetProperty("facets", out var facetsProp) && facetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var facet in facetsProp.EnumerateArray())
                {
                    var field = facet.GetProperty("field").GetString();
                    var detail = new FacetDetail
                    {
                        Field = field,
                        Label = facet.GetProperty("label").GetString(),
                        Multiple = facet.GetProperty("multiple").GetString(),
                        Collapse = facet.TryGetProperty("collapse", out var collapseProp) && collapseProp.GetInt32() == 1
                    };

                    if (facet.TryGetProperty("values", out var valuesProp) && valuesProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var val in valuesProp.EnumerateArray())
                        {
                            detail.Values.Add(new FacetValue
                            {
                                Value = val.GetProperty("value").GetString(),
                                Label = val.GetProperty("label").GetString(),
                                Count = val.GetProperty("count").GetInt32()
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(field))
                        facets[field] = detail;
                }
            }

            return facets;
        }

        private List<SortOption> ParseSortOptions(JsonElement root)
        {
            var sortOptions = new List<SortOption>();

            if (root.TryGetProperty("sortOptions", out var sortOptionsProp) && sortOptionsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var sortOption in sortOptionsProp.EnumerateArray())
                {
                    sortOptions.Add(new SortOption
                    {
                        Type = sortOption.GetProperty("type").GetString(),
                        Field = sortOption.GetProperty("field").GetString(),
                        Direction = sortOption.GetProperty("direction").GetString(),
                        Label = sortOption.GetProperty("label").GetString()
                    });
                }
            }

            return sortOptions;
        }
    }
}