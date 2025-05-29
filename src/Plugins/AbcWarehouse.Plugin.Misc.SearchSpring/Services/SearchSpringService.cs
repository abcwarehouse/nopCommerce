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

        public async Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null, string siteId = "4lt84w")
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query must not be null or empty.", nameof(query));

            var client = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}/api/search/search.json?" +
                      $"q={WebUtility.UrlEncode(query)}" +
                      $"&resultsFormat=json" +
                      $"&resultsPerPage=24" +
                      $"&page=1" +
                      $"&redirectResponse=minimal";

            if (!string.IsNullOrEmpty(sessionId))
                url += $"&ss-sessionId={WebUtility.UrlEncode(sessionId)}";

            url += $"&siteId={WebUtility.UrlEncode(siteId)}";

            Console.WriteLine($"[SearchSpring] Requesting URL: {url}");

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[SearchSpring] Response ({(int)response.StatusCode}): {json}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Searchspring returned error {response.StatusCode}: {json}\nURL: {url}");
            }

            try
            {
                var productList = new List<SearchSpringProductModel>();
                int page = 1, pageSize = 24, totalResults = 0;

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Parse products
                if (root.TryGetProperty("results", out var resultsElement))
                {
                    foreach (var item in resultsElement.EnumerateArray())
                    {
                        var model = new SearchSpringProductModel
                        {
                            Name = item.GetProperty("name").GetString(),
                            ProductUrl = item.GetProperty("url").GetString(),
                            ImageUrl = item.GetProperty("imageUrl").GetString(),
                            Price = item.TryGetProperty("price", out var priceProp) ? priceProp.GetString() : ""
                        };

                        productList.Add(model);
                    }
                }

                // Parse pagination
                if (root.TryGetProperty("pagination", out var pagination))
                {
                    page = pagination.GetProperty("currentPage").GetInt32();
                    pageSize = pagination.GetProperty("pageSize").GetInt32();
                    totalResults = pagination.GetProperty("totalResults").GetInt32();
                }

                return new SearchResultModel
                {
                    Results = productList,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalResults = totalResults
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
