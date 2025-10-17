using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using System;
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
        private const string BeaconApiUrl = "https://beacon.searchspring.io/api/recommendations/beacon";


        public SearchSpringBeaconController(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("event")]
        public async Task<IActionResult> SendEvent([FromBody] object eventData)
        {
            if (eventData == null)
                return BadRequest("Missing event data");

            try
            {
                // Ensure the data is posted to Searchspring exactly as JSON
                var content = new StringContent(eventData.ToString(), Encoding.UTF8, "application/json");

                // Keep referrer/origin headers intact
                _httpClient.DefaultRequestHeaders.Referrer = Request.GetTypedHeaders().Referer;
                _httpClient.DefaultRequestHeaders.Add("Origin", Request.Headers["Origin"].ToString());

                var response = await _httpClient.PostAsync(BeaconApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Information,
                    "[Searchspring Beacon]",
                    $"Payload: {eventData}, Response: {responseBody}"
                );

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseBody);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(
                    Nop.Core.Domain.Logging.LogLevel.Error,
                    "[Searchspring Beacon Error]",
                    ex.ToString()
                );
                return StatusCode(500, ex.Message);
            }
        }
    }

}
