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
        private readonly string _baseUrl = "https://4lt84w.a.searchspring.io";

        public SearchSpringService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null, string siteId = "4lt84w", int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query must not be null or empty.", nameof(query));

            var client = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}/api/search/search.json?" +
                      $"q={WebUtility.UrlEncode(query)}" +
                      "&resultsFormat=json" +
                      "&resultsPerPage=24" +
                      $"&page={page}" +
                      "&redirectResponse=minimal";

            if (!string.IsNullOrEmpty(sessionId))
                url += "&ss-sessionId=" + WebUtility.UrlEncode(sessionId);

            url += "&siteId=" + WebUtility.UrlEncode(siteId);

            Console.WriteLine($"[SearchSpring] Requesting URL: {url}");

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[SearchSpring] Response ({(int)response.StatusCode}): {json}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Searchspring returned error {response.StatusCode}: {json}\nURL: {url}");

            try
            {
                var productList = new List<SearchSpringProductModel>();
                int currentPage = 1, pageSize = 24, totalResults = 0;
                var facets = new Dictionary<string, FacetDetail>();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

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
                        productList.Add(model);
                    }
                }

                if (root.TryGetProperty("pagination", out var pagination) && pagination.ValueKind == JsonValueKind.Object)
                {
                    currentPage = pagination.TryGetProperty("currentPage", out var pageProp) ? pageProp.GetInt32() : 1;
                    pageSize = pagination.TryGetProperty("pageSize", out var sizeProp) ? sizeProp.GetInt32() : 24;
                    totalResults = pagination.TryGetProperty("totalResults", out var totalProp) ? totalProp.GetInt32() : 0;
                }

                if (root.TryGetProperty("facets", out var facetsProp) && facetsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var facet in facetsProp.EnumerateObject())
                    {
                        var value = facet.Value;
                        var detail = new FacetDetail
                        {
                            Multiple = value.TryGetProperty("multiple", out var multipleProp) ? multipleProp.GetString() : "",
                            Field = value.TryGetProperty("type", out var fieldProp) ? fieldProp.GetString() : "",
                            Label = value.TryGetProperty("label", out var labelProp) ? labelProp.GetString() : "",
                            Collapse = value.TryGetProperty("collapse", out var collapseProp) && collapseProp.GetInt32() == 1
                        };

                        if (value.TryGetProperty("values", out var valuesProp) && valuesProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var val in valuesProp.EnumerateArray())
                            {
                                detail.Values.Add(new FacetValue
                                {
                                    Value = val.TryGetProperty("value", out var v) ? v.GetString() : "",
                                    Label = val.TryGetProperty("label", out var l) ? l.GetString() : "",
                                    Count = val.TryGetProperty("count", out var c) ? c.GetInt32() : 0
                                });
                            }
                        }

                        facets[facet.Name] = detail;
                    }
                }
                else
                {
                    Console.WriteLine("[SearchSpring] 'facets' object not found or malformed.");
                }

                return new SearchResultModel
                {
                    Results = productList,
                    PageNumber = currentPage,
                    PageSize = pageSize,
                    TotalResults = totalResults,
                    Facets = facets
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SearchSpring] JSON Deserialization failed: {ex.Message}");
                throw new Exception("Failed to parse Searchspring response.", ex);
            }
        }
    }
}
