﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Services.Custom;

public class ListrakApiService : IListrakApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ListrakApiSettings _listrakApiSettings;

    public ListrakApiService(HttpClient httpClient, IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<ListrakApiSettings> listrakApiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://api.listrak.com");
        _listrakApiSettings = listrakApiSettings.Value;
    }

    public async Task<string> GetTokenAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.listrak.com/OAuth2/Token")
            {
                // Content = new FormUrlEncodedContent(new Dictionary<string, string>
                // {
                //     { "client_id", _listrakApiSettings.ClientId },
                //     { "client_secret", _listrakApiSettings.ClientSecret },
                //     { "grant_type", "client_credentials" }
                // })
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", "ao1xkc57sz7t1dw1qawh" },
                    { "client_secret", "rDpBSv2PMMrpo2Nso0AAyFqiag1U395bYV4ltx1vhIE" },
                    { "grant_type", "client_credentials" }
                })
            };

            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve token. Status Code: {response.StatusCode}, Response: {content}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);
            return tokenResponse?.AccessToken ?? throw new Exception("Access token is null.");
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve the API token.", ex);
        }
    }

    public ApiResponse SendBillingAddress(string token, Address billingAddress, bool isSmsChecked, bool isMarketingChecked)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage smsResponse = null;
        HttpResponseMessage marketingSmsResponse = null;

        if(isSmsChecked)
        {
            smsResponse = SmsSub(token, billingAddress, client);
        }

        if(isMarketingChecked)
        {
            marketingSmsResponse = MarketingSub(token, billingAddress, client);
        }

        if(smsResponse.IsSuccessStatusCode == marketingSmsResponse.IsSuccessStatusCode || smsResponse.IsSuccessStatusCode == false)
        {
            return new ApiResponse
            {
                IsSuccess = smsResponse.IsSuccessStatusCode,
                Message = smsResponse.ReasonPhrase
            };
        }
        else
        {
            return new ApiResponse
            {
                IsSuccess = marketingSmsResponse.IsSuccessStatusCode,
                Message = marketingSmsResponse.ReasonPhrase
            };
        }        
        
    }

    public HttpResponseMessage SmsSub(string token, Address billingAddress, HttpClient client)
    {
        var listrakData = new 
        {
            ListrakData = new 
            {
                ShortCodeId = "1026",
                PhoneNumber = billingAddress.PhoneNumber,
                PhoneListId = "152"
            }
        };

        var response = client.PostAsJsonAsync($"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ListrakData.ShortCodeId}/Contact/{billingAddress.PhoneNumber}/PhoneList/{listrakData.ListrakData.PhoneListId}", listrakData).Result;

        return response;
    }
    public HttpResponseMessage MarketingSub(string token, Address billingAddress, HttpClient client)
    {
        var listrakData = new 
        {
            ListrakData = new 
            {
                ShortCodeId = "1026",
                PhoneNumber = billingAddress.PhoneNumber,
                PhoneListId = "151"
            }
        };

        var response = client.PostAsJsonAsync($"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ListrakData.ShortCodeId}/Contact/{billingAddress.PhoneNumber}/PhoneList/{listrakData.ListrakData.PhoneListId}", listrakData).Result;

        return response;
    }

    public ApiResponse CheckSubList(String phoneNumber)
    {
        var token = GetTokenAsync();
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.ToString());

        var listrakData = new 
        {
            ListrakData = new 
            {
                SenderCode = "1026",
                PhoneNumber = phoneNumber
            }
        };

        var response = client.PostAsJsonAsync($"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ListrakData.SenderCode}/Contact/{phoneNumber}", listrakData).Result;

        return new ApiResponse
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = response.ReasonPhrase
        };
    }

    public ApiResponse UnsubListrak(string token, Address billingAddress)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listrakData = new 
        {
            ListrakData = new 
            {
                ShortCodeId = "1026",
                PhoneNumber = billingAddress.PhoneNumber,
                PhoneListId = "152"
            }
        };

        var response = client.PostAsJsonAsync($"https://api.listrak.com/sms/v1/ShortCode/{listrakData.ListrakData.ShortCodeId}/ContactUnsubscribe/{billingAddress.PhoneNumber}/PhoneList/{listrakData.ListrakData.PhoneListId}", listrakData).Result;

        return new ApiResponse
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = response.ReasonPhrase
        };
    }

}
