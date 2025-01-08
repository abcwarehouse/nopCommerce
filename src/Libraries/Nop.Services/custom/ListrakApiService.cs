using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Custom;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class ListrakApiService : IListrakApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ListrakApiService(HttpClient httpClient, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://api.listrak.com");
    }

    /*public string GetToken()
    {
        var tokenEndpoint = "/OAuth2/Token"; // Relative path for token API
        var credentials = new
        {
            Username = "ao1xkc57sz7t1dw1qawh",
            Password = "rDpBSv2PMMrpo2Nso0AAyFqiag1U395bYV4ltx1vhIE"
        };

        try
        {
            var response = _httpClient.PostAsJsonAsync(tokenEndpoint, credentials).Result;

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    return tokenResponse.AccessToken;
                }
                throw new Exception("Token is null or empty in the API response.");
            }
            else
            {
                // Log or handle API failure
                throw new Exception($"API request failed with status code {response.StatusCode} and reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // Log the detailed exception for troubleshooting
            Console.WriteLine($"Exception in GetToken: {ex.Message}");
            throw new Exception("Failed to retrieve the API token.", ex);
        }
    }*/ 

    public async Task<string> GetTokenAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.listrak.com/OAuth2/Token");
            request.Content = new StringContent(JsonConvert.SerializeObject(new
            {
                username = "ao1xkc57sz7t1dw1qawh",
                password = "rDpBSv2PMMrpo2Nso0AAyFqiag1U395bYV4ltx1vhIE"
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

            return tokenResponse?.AccessToken ?? throw new Exception("Access token is null.");
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve the API token.", ex);
        }
    }

    public ApiResponse SendBillingAddress(string token, Address billingAddress, bool isCheckboxChecked)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer" + token);

        var data = new
        {
            Address = new
            {
                billingAddress.FirstName,
                billingAddress.LastName,
                billingAddress.Email,
                billingAddress.Address1,
                billingAddress.City,
                billingAddress.ZipPostalCode,
                billingAddress.CountryId,
            },
            CheckboxChecked = isCheckboxChecked
        };

        var response = client.PostAsJsonAsync("https://api.listrak.com/sms/v1", data).Result;

        return new ApiResponse
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = response.ReasonPhrase
        };
    }
}
