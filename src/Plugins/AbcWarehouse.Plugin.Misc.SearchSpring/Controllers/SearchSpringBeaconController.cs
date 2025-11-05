using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
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
        private readonly HttpClient _httpClient;

        private const string BeaconApiUrl = "https://beacon.searchspring.io/api/beacon";

        public SearchSpringBeaconController(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("events")]
        public async Task<IActionResult> SendEvent([FromBody] object eventData)
        {
            if (eventData == null)
                return BadRequest("Missing event data");

            try
            {
                // Serialize the JsonElement properly
                var jsonString = JsonSerializer.Serialize(eventData);
                
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BeaconApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    "[Searchspring Beacon]",
                    $"Payload: {jsonString}, Response: {responseBody}, StatusCode: {response.StatusCode}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    await _logger.InsertLogAsync(
                        Nop.Core.Domain.Logging.LogLevel.Warning,
                        "[Searchspring Beacon - Non-Success Response]",
                        $"StatusCode: {response.StatusCode}, Response: {responseBody}"
                    );
                    return StatusCode((int)response.StatusCode, responseBody);
                }

                return Ok(new { success = true, response = responseBody });
            }
            catch (System.Exception ex)
            {
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Error,
                    "[Searchspring Beacon Error]",
                    $"Exception: {ex.Message}, StackTrace: {ex.StackTrace}"
                );
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}