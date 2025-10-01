using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;
using System.Net.Http;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using Nop.Services.Logging;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    public class SearchSpringController : Controller
    {
        private readonly ISearchSpringService _searchSpringService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly SearchSpringSettings _settings;

        public SearchSpringController(ISearchSpringService searchSpringService,
                                      IHttpClientFactory httpClientFactory,
                                      ILogger logger,
                                      ISettingService settingService,
                                      ILocalizationService localizationService,
                                      INotificationService notificationService,
                                      SearchSpringSettings settings)
        {
            _searchSpringService = searchSpringService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settings = settings;
        }

        [HttpGet]
        public async Task<IActionResult> Query(string term)
        {
            var sessionId = GetSearchSpringSessionId();
            var results = await _searchSpringService.SearchAsync(term, sessionId: sessionId);

            if (!string.IsNullOrEmpty(results.RedirectResponse))
                return Redirect(results.RedirectResponse);

            return Json(results);
        }

        [HttpGet]
        [Route("searchspring/results")]
        [Route("search/results")]
        public async Task<IActionResult> Results(string q, int page = 1, string sort = null, double? lat = null, double? lng = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search term cannot be empty.");

            var sessionId = GetOrCreateSearchSpringSessionId(HttpContext);
            var siteId = "4lt84w";
            var filters = new Dictionary<string, List<string>>();

            foreach (var key in HttpContext.Request.Query.Keys)
            {
                if (key.StartsWith("filter["))
                {
                    var field = key.Substring(7, key.Length - 8);
                    var values = HttpContext.Request.Query[key].ToList();

                    if (!filters.ContainsKey(field))
                        filters[field] = new List<string>();

                    filters[field].AddRange(values);
                }
            }

            await _logger.InformationAsync($"Geo received: lat={lat?.ToString() ?? "null"}, lng={lng?.ToString() ?? "null"}");
            await _logger.InformationAsync($"[SearchSpring Controller] Query: {q}, Page: {page}, Sort: {sort}");
            await _logger.InformationAsync($"Full request URL: {HttpContext.Request.QueryString}");
            var results = await _searchSpringService.SearchAsync(
                q, sessionId: sessionId, siteId: siteId, page: page, filters: filters, sort: sort, latitude: lat, longitude: lng
            );

            if (!string.IsNullOrEmpty(results.RedirectResponse))
            {
                return Redirect(results.RedirectResponse); // performs 302 HTTP redirect
            }

            // If an ABC item number was provided and one response is given,
            // redirect to the product URL
            if (results.Results.Count() == 1 && q.All(char.IsDigit))
            {
                var productUrl = results.Results.First().ProductUrl;
                return Redirect(productUrl);
            }

            results.PageNumber = page;
            results.Query = q;

            var modelJson = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });

            if (_settings.IsDebugMode)
            {
                await _logger.InformationAsync($"SearchSpring Results JSON:\n{modelJson}");
            }

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
                return Json(new { results = System.Net.WebUtility.HtmlEncode(content) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred while fetching suggestions. Message: " + ex.Message });
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
                return StatusCode(500, new { error = "An internal server error occurred while fetching autocomplete results. Message: " + ex.Message });
            }
        }
        
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public ActionResult Configure()
        {
            return View(
                "~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Configure.cshtml",
                _settings.ToModel());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            await _settingService.SaveSettingAsync(SearchSpringSettings.FromModel(model));

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}