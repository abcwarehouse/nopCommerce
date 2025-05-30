using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using System.Net.Http;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;


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

            // Generate/retrieve session ID from cookie or context
            var sessionId = GetOrCreateSearchSpringSessionId(HttpContext);
            var siteId = "4lt84w";

            var results = await _searchSpringService.SearchAsync(q, sessionId: sessionId, siteId: siteId);
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Results.cshtml", results);
        }
        private string GetOrCreateSearchSpringSessionId(HttpContext context)
        {
            const string cookieKey = "ss-sessionId";

            if (context.Request.Cookies.TryGetValue(cookieKey, out var existingSessionId))
            {
                return existingSessionId;
            }

            var newSessionId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(cookieKey, newSessionId, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });

            return newSessionId;
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
            var siteId = "4lt84w";
            var suggestUrl = $"https://{siteId}.a.searchspring.io/api/suggest/search?q={WebUtility.UrlEncode(q)}";


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
