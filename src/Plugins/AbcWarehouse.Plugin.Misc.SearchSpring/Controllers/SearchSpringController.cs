using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Nop.Services.Logging;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    public class SearchSpringController : BasePluginController
    {
        private readonly ISearchSpringService _searchSpringService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly SearchSpringSettings _settings;

        private const string SiteId = "4lt84w";
        private const string SessionCookieKey = "ss-sessionId";

        public SearchSpringController(
            ISearchSpringService searchSpringService,
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
        public async Task<IActionResult> Results(string q, int page = 1, string sort = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search term cannot be empty.");

            var sessionId = GetOrCreateSearchSpringSessionId(HttpContext);
            var filters = ParseFilters(HttpContext.Request.Query);

            var results = await _searchSpringService.SearchAsync(q, sessionId: sessionId, siteId: SiteId, page: page, filters: filters, sort: sort);

            if (!string.IsNullOrEmpty(results.RedirectResponse))
                return Redirect(results.RedirectResponse);

            if (results.Results.Count() == 1 && q.All(char.IsDigit))
                return Redirect(results.Results.First().ProductUrl);

            results.PageNumber = page;
            results.Query = q;

            if (_settings.IsDebugMode)
            {
                var modelJson = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                await _logger.InformationAsync($"SearchSpring Results JSON:\n{modelJson}");
            }

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Results.cshtml", results);
        }

        [HttpPost]
        public async Task<IActionResult> GetPersonalizedRecommendations([FromBody] PersonalizationRequestModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ShopperId))
                return BadRequest("Invalid shopperId.");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Site-Id", SiteId);

                var payload = new
                {
                    shopperId = model.ShopperId,
                    context = new
                    {
                        page = new { type = model.PageType ?? "home" }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://api.searchspring.io/api/personalized/v1/recommendations", content);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return Content(jsonResponse, "application/json");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("SearchSpring personalization error", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("searchspring/suggest")]
        public async Task<IActionResult> Suggest(string q, string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            var url = $"https://{SiteId}.a.searchspring.io/api/suggest/query?q={HttpUtility.UrlEncode(q)}";

            if (!string.IsNullOrWhiteSpace(userId))
                url += $"&userId={HttpUtility.UrlEncode(userId)}";
            if (!string.IsNullOrWhiteSpace(sessionId))
                url += $"&sessionId={HttpUtility.UrlEncode(sessionId)}";
            url += $"&siteId={SiteId}";

            return await ProxyExternalRequest(url, "suggest");
        }

        [HttpGet("searchspring/autocomplete")]
        public async Task<IActionResult> Autocomplete(string q, string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            var url = $"https://{SiteId}.a.searchspring.io/api/search/autocomplete?q={HttpUtility.UrlEncode(q)}";

            if (!string.IsNullOrWhiteSpace(userId))
                url += $"&userId={HttpUtility.UrlEncode(userId)}";
            if (!string.IsNullOrWhiteSpace(sessionId))
                url += $"&sessionId={HttpUtility.UrlEncode(sessionId)}";
            url += $"&siteId={SiteId}";

            return await ProxyExternalRequest(url, "autocomplete");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public ActionResult Configure()
        {
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Configure.cshtml", _settings.ToModel());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            await _settingService.SaveSettingAsync(SearchSpringSettings.FromModel(model));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }

        #region Private helpers

        private string GetSearchSpringSessionId()
        {
            Request.Cookies.TryGetValue("_ss_s", out var sessionId);
            return sessionId;
        }

        private string GetOrCreateSearchSpringSessionId(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(SessionCookieKey, out var existingSessionId))
                return existingSessionId;

            var newSessionId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(SessionCookieKey, newSessionId, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });

            return newSessionId;
        }

        private Dictionary<string, List<string>> ParseFilters(IQueryCollection query)
        {
            var filters = new Dictionary<string, List<string>>();

            foreach (var key in query.Keys)
            {
                if (key.StartsWith("filter["))
                {
                    var field = key.Substring(7, key.Length - 8);
                    var values = query[key].ToList();

                    if (!filters.ContainsKey(field))
                        filters[field] = new List<string>();

                    filters[field].AddRange(values);
                }
            }

            return filters;
        }

        private async Task<IActionResult> ProxyExternalRequest(string url, string context)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { error = $"SearchSpring {context} API error: {errorContent}" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error in SearchSpring {context} proxy", ex);
                return StatusCode(500, new { error = $"An internal error occurred while fetching {context} data." });
            }
        }

        #endregion
    }
}
