using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using System.Net.Http;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using Nop.Services.Logging;
using System.Web; 


namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    public class SearchSpringController : Controller
    {
        private readonly ISearchSpringService _searchSpringService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger; // Inject ILogger for better error reporting

        // Add a logger to the constructor
        public SearchSpringController(ISearchSpringService searchSpringService,
                                      IHttpClientFactory httpClientFactory,
                                      ILogger logger)
        {
            _searchSpringService = searchSpringService;
            _httpClientFactory = httpClientFactory;
            _logger = logger; // Initialize logger
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
            return null;
        }

        [Route("searchspring/suggest")]
        [HttpGet]
        public async Task<IActionResult> Suggest(string q, string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var siteId = "4lt84w";
                var suggestUrl = $"https://{siteId}.a.searchspring.io/api/suggest/query?q={HttpUtility.UrlEncode(q)}";

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    suggestUrl += $"&userId={HttpUtility.UrlEncode(userId)}";
                }
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    suggestUrl += $"&sessionId={HttpUtility.UrlEncode(sessionId)}";
                }
                if (!string.IsNullOrWhiteSpace(siteId))
                {
                    suggestUrl += $"&siteId={HttpUtility.UrlEncode(siteId)}";
                }

                var response = await client.GetAsync(suggestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log the error for server-side debugging
                    return StatusCode((int)response.StatusCode, new { error = $"Searchspring Suggest API error: {errorContent}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred while fetching suggestions." });
            }
        }
        
        [Route("searchspring/autocomplete")]
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string q, string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var siteId = "4lt84w";
                var autocompleteUrl = $"https://{siteId}.a.searchspring.io/api/search/autocomplete?q={HttpUtility.UrlEncode(q)}";

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    autocompleteUrl += $"&userId={HttpUtility.UrlEncode(userId)}";
                }
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    autocompleteUrl += $"&sessionId={HttpUtility.UrlEncode(sessionId)}";
                }
                if (!string.IsNullOrWhiteSpace(siteId))
                {
                    autocompleteUrl += $"&siteId={HttpUtility.UrlEncode(siteId)}";
                }

                var response = await client.GetAsync(autocompleteUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { error = $"Searchspring Autocomplete API error: {errorContent}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred while fetching autocomplete results." });
            }
        }

    }
}