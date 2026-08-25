using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AbcWarehouse.Plugin.Widgets.Listrak;
using AbcWarehouse.Plugin.Widgets.Listrak.Models;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

public class ListrakService : IListrakService
{
    private const string ShortCodeId = "1026";

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

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

    public async Task<HttpResponseMessage> SubscribePhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null, string emailAddress = null)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listrakData = new PhoneListContactModel
        {
            ShortCodeId = ShortCodeId,
            PhoneNumber = phoneNumber,
            PhoneListId = "151",
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = emailAddress
        };

        return await client.PostAsJsonAsync(
            $"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ShortCodeId}/PhoneList/{listrakData.PhoneListId}/Contact",
            listrakData
        );
    }

    public async Task<HttpResponseMessage> SubscribeOrEnrichPhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null, string emailAddress = null)
    {
        var subscribeResponse = await SubscribePhoneNumberAsync(phoneNumber, firstName, lastName, emailAddress);

        if (subscribeResponse.IsSuccessStatusCode)
            return subscribeResponse;

        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(emailAddress))
            return subscribeResponse; // nothing to enrich with, and no point spending an extra API call

        try
        {
            var content = await subscribeResponse.Content.ReadAsStringAsync();
            var error = SystemJsonSerializer.Deserialize<ListrakApiErrorResponse>(content, CaseInsensitiveJson);

            if (error?.Error != "ERROR_PHONE_NUMBER_FOUND")
                return subscribeResponse; // some other failure (invalid/blocked/opted-out) - nothing more to do here

            var existing = await GetContactAsync(phoneNumber);
            if (existing == null)
                return subscribeResponse;

            var needsUpdate = false;

            if (string.IsNullOrWhiteSpace(existing.FirstName) && !string.IsNullOrWhiteSpace(firstName))
            {
                existing.FirstName = firstName;
                needsUpdate = true;
            }

            if (string.IsNullOrWhiteSpace(existing.LastName) && !string.IsNullOrWhiteSpace(lastName))
            {
                existing.LastName = lastName;
                needsUpdate = true;
            }

            if (string.IsNullOrWhiteSpace(existing.EmailAddress) && !string.IsNullOrWhiteSpace(emailAddress))
            {
                existing.EmailAddress = emailAddress;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                // Echo every other field back exactly as Listrak returned it so this update can
                // only ever fill in missing name/email - it can't clear/replace birthday, postal
                // code, opt-out status, or segmentation data.
                existing.PhoneNumber = phoneNumber;
                await UpdateContactAsync(existing);
            }
        }
        catch
        {
            // Enrichment is best-effort; the original subscribe response below is still what matters.
        }

        return subscribeResponse;
    }

    /// <summary>
    /// Looks up a contact by phone number. This resource isn't list-scoped - it returns null on a
    /// 404 (no such contact for this sender code) or any other non-success response.
    /// </summary>
    private async Task<SmsContactSubscriptionDetails> GetContactAsync(string phoneNumber)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"https://api.listrak.com/sms/v1/ShortCode/{ShortCodeId}/Contact/{phoneNumber}");

        if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var wrapper = SystemJsonSerializer.Deserialize<GetContactResponse>(content, CaseInsensitiveJson);

        return wrapper?.Data;
    }

    private async Task<HttpResponseMessage> UpdateContactAsync(SmsContactSubscriptionDetails contact)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.PutAsJsonAsync(
            $"https://api.listrak.com/sms/v1/ShortCode/{ShortCodeId}/Contact/{contact.PhoneNumber}",
            contact
        );
    }
}
