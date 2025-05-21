using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public class SearchSpringService
    {
        private readonly HttpClient _httpClient;

        public SearchSpringService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SearchSpringProductModel>> SearchAsync(string query)
        {
            var response = await _httpClient.GetAsync($"https://api.searchspring.net/api/search/search.json?q={query}&siteId=4lt84w");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<SearchSpringResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return parsed?.Results ?? new List<SearchSpringProductModel>();
        }
    }
}
