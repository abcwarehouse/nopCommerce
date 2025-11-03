using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    [Route("searchspring/beacon")]
    public class SearchSpringBeaconController : Controller
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BeaconApiUrl = "https://beacon.searchspring.io/beacon";
        private readonly HttpClient _httpClient;
        private readonly SearchSpringSettings _searchSpringSettings;

        public SearchSpringBeaconController(ILogger logger, IHttpClientFactory httpClientFactory, SearchSpringSettings searchSpringSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
            _searchSpringSettings = searchSpringSettings;
        }

        [HttpPost("event")]
        public async Task<IActionResult> SendEvent([FromBody] object eventData)
        {
            if (eventData == null)
                return BadRequest("Missing event data");

            try
            {
                // Create a fresh HttpClient for each request (from factory)
                var httpClient = _httpClientFactory.CreateClient();

                // Properly serialize the JSON
                var jsonPayload = eventData.ToString();
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Log the payload for debugging
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    "[Searchspring Beacon] Request",
                    $"Sending payload: {jsonPayload}"
                );

                // Set headers on the request message instead of the client
                var request = new HttpRequestMessage(HttpMethod.Post, BeaconApiUrl)
                {
                    Content = content
                };

                // Add referrer if available
                var referer = Request.GetTypedHeaders().Referer;
                if (referer != null)
                {
                    request.Headers.Referrer = referer;
                }

                // Add origin if available
                if (Request.Headers.ContainsKey("Origin"))
                {
                    var origin = Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin))
                    {
                        request.Headers.TryAddWithoutValidation("Origin", origin);
                    }
                }

                // Add User-Agent
                if (Request.Headers.ContainsKey("User-Agent"))
                {
                    var userAgent = Request.Headers["User-Agent"].ToString();
                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                    }
                }

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (_searchSpringSettings.IsDebugMode)
                {
                    await _logger.InsertLogAsync(
                        Nop.Core.Domain.Logging.LogLevel.Information,
                        "[Searchspring Beacon]",
                        $"Payload: {eventData}, Response: {responseBody}"
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    await _logger.InsertLogAsync(
                        Nop.Core.Domain.Logging.LogLevel.Warning,
                        "[Searchspring Beacon] Failed",
                        $"Status: {response.StatusCode}, Response: {responseBody}"
                    );
                    return StatusCode((int)response.StatusCode, responseBody);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Error,
                    "[Searchspring Beacon Error]",
                    $"Exception: {ex.Message}\nStack: {ex.StackTrace}"
                );
                return StatusCode(500, ex.Message);
            }
        }
    }
}