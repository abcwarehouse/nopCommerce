using System;
using System.Text.Json;
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
        try
        {
            if (!Regex.IsMatch(request.PhoneNumber, @"^\d{10}$"))
                return BadRequest(new { message = "Invalid phone number." });

            var response = await _listrakService.SubscribePhoneNumberAsync(request.PhoneNumber);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Successfully subscribed!" });
            }

            var content = await response.Content.ReadAsStringAsync();
            var friendlyMessage = GetFriendlyErrorMessage(content);
            return StatusCode((int)response.StatusCode, new { message = friendlyMessage });
        }
        catch (Exception ex)
        {
            // TODO: Use your plugin's logger
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    private string GetFriendlyErrorMessage(string apiResponse)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<ListrakErrorResponse>(apiResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return errorResponse?.Error switch
            {
                "ERROR_PHONE_NUMBER_FOUND" => "This phone number is already subscribed.",
                "ERROR_INVALID_PHONE_NUMBER" => "Please enter a valid phone number.",
                "ERROR_PHONE_NUMBER_OPTED_OUT" => "This phone number has previously opted out. Please text JOIN to resubscribe.",
                "ERROR_PHONE_NUMBER_BLOCKED" => "This phone number cannot be subscribed at this time.",
                _ => "Unable to subscribe. Please try again later."
            };
        }
        catch
        {
            return "Unable to subscribe. Please try again later.";
        }
    }

    private class ListrakErrorResponse
    {
        public int Status { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }
}

public class MarketingSmsModel
{
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
}
