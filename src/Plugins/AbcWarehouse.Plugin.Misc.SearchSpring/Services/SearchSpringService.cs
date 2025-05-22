using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public class SearchSpringService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://4lt84w.a.searchspring.io";

        public SearchSpringService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> SearchAsync(string query)
        {
            var client = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}/api/search/search.json?q={WebUtility.UrlEncode(query)}&resultsFormat=json&resultsPerPage=24&page=1&redirectResponse=minimal";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Searchspring returned error {response.StatusCode}: {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }

}
