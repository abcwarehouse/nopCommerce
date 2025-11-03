using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    [Route("searchspring/beacon")]
    public class SearchSpringBeaconController : Controller
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly SearchSpringSettings _searchSpringSettings;

        private const string BeaconApiUrl = "https://beacon.searchspring.io/api/beacon";

        public SearchSpringBeaconController(ILogger logger, IHttpClientFactory httpClientFactory, SearchSpringSettings searchSpringSettings)
        {
            _logger = logger;
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
                var content = new StringContent(eventData.ToString(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BeaconApiUrl, content);
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
                    return StatusCode((int)response.StatusCode, responseBody);

                return Ok(new { success = true });
            }
            catch (System.Exception ex)
            {
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Error,
                    "[Searchspring Beacon Error]",
                    ex.Message
                );
                return StatusCode(500, ex.Message);
            }
        }
    }
}
