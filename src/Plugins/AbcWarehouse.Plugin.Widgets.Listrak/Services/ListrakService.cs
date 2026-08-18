using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AbcWarehouse.Plugin.Widgets.Listrak.Models;

public class ListrakService : IListrakService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ListrakSettings _settings;

    public ListrakService(IHttpClientFactory httpClientFactory, ListrakSettings settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    public async Task<string> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
            throw new Exception("Widgets.Listrak: ClientId/ClientSecret are not configured. Set them in Admin > Widgets.Listrak > Configure.");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.listrak.com/OAuth2/Token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "grant_type", "client_credentials" }
            })
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Token error: {response.StatusCode} - {content}");

        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);
        return tokenResponse?.AccessToken ?? throw new Exception("Access token is null.");
    }

    public async Task<HttpResponseMessage> SubscribePhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listrakData = new PhoneListContactModel
        {
            ShortCodeId = "1026",
            PhoneNumber = phoneNumber,
            PhoneListId = "151",
            FirstName = firstName,
            LastName = lastName
        };

        return await client.PostAsJsonAsync(
            $"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ShortCodeId}/PhoneList/{listrakData.PhoneListId}/Contact",
            listrakData
        );
    }
}
