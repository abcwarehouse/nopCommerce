using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/marketingSms")]
public class MarketingController : ControllerBase
{
    private readonly IListrakService _listrakService;

    public MarketingController(IListrakService listrakService)
    {
        _listrakService = listrakService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] MarketingSmsModel request)
    {
        if (!Regex.IsMatch(request.PhoneNumber, @"^\d{10}$"))
            return BadRequest(new { message = "Invalid phone number." });

        var response = await _listrakService.SubscribePhoneNumberAsync(request.PhoneNumber);

        if (response.IsSuccessStatusCode)
        {
            return Ok(new { message = "Successfully subscribed!" });
        }

        return StatusCode((int)response.StatusCode, new { message = "Subscription failed." });
    }
}

public class MarketingSmsModel
{
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
}
