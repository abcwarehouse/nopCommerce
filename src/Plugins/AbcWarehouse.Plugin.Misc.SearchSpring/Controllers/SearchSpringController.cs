using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using System.Net.Http;
using System.Net;


namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    public class SearchSpringController : Controller
    {
        private readonly ISearchSpringService _searchSpringService;
        private readonly IHttpClientFactory _httpClientFactory;


        public SearchSpringController(ISearchSpringService searchSpringService, IHttpClientFactory httpClientFactory)
        {
            _searchSpringService = searchSpringService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Query(string term)
        {
            var sessionId = GetSearchSpringSessionId();
            var results = await _searchSpringService.SearchAsync(term, sessionId: sessionId);
            return Json(results);
        }

        public async Task<IActionResult> Results(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search term cannot be empty.");

            var sessionId = GetSearchSpringSessionId();
            var results = await _searchSpringService.SearchAsync(q, sessionId: sessionId);
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Results.cshtml", results);
        }

        private string GetSearchSpringSessionId()
        {
            if (Request.Cookies.TryGetValue("_ss_s", out var sessionId))
                return sessionId;

            // Fallback: generate one if needed (optional)
            // sessionId = Guid.NewGuid().ToString();
            // Response.Cookies.Append("_ss_s", sessionId);
            return sessionId;
        }

        [Route("searchspring/suggest")]
        [HttpGet]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            var client = _httpClientFactory.CreateClient();
            var suggestUrl = $"https://4lt84w.a.searchspring.io/api/suggest/search?q={WebUtility.UrlEncode(q)}";

            var response = await client.GetAsync(suggestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest($"Searchspring Suggest API error: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

    }
}
