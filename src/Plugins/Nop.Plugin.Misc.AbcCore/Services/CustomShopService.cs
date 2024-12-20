using SevenSpikes.Nop.Plugins.StoreLocator.Services;
using System.Linq;
using SevenSpikes.Nop.Plugins.StoreLocator.Domain.Shops;
using Nop.Services.Events;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Core.Caching;
using SevenSpikes.Nop.EntitySettings.Services.EntitySettings;
using Nop.Core;
using Nop.Services.Stores;
using SevenSpikes.Nop.Plugins.StoreLocator.Domain;
using Nop.Services.Customers;
using Nop.Services.Caching;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using Nop.Services.Common;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public class CustomShopService : ShopService, ICustomShopService
    {
        private readonly IRepository<ShopAbc> _shopAbcRepository;
        private readonly IRepository<Shop> _shopRepository;

        public CustomShopService(
            IRepository<ShopImage> shopImageRepository,
            IRepository<Shop> shopRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IStaticCacheManager staticCacheManager,
            IEntitySettingService entitySettingService,
            IEventPublisher eventPublisher,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWebHelper webHelper,
            IWorkContext workContext,
            StoreLocatorSettings storeLocatorSettings,
            ICustomerService customerService,
            IRepository<ShopAbc> shopAbcRepository
        ) : base(shopImageRepository, shopRepository, urlRecordRepository,
                 staticCacheManager, entitySettingService, eventPublisher,
                 storeContext, storeMappingService, webHelper, workContext,
                 storeLocatorSettings, customerService)
        {
            _shopAbcRepository = shopAbcRepository;
            _shopRepository = shopRepository;
        }

        public ShopAbc GetShopAbcByShopId(int shopId)
        {
            return _shopAbcRepository.Table.Where(sa => sa.ShopId == shopId).FirstOrDefault();
        }

        public Shop GetShopByAbcBranchId(string branchId)
        {
            var shopAbc = _shopAbcRepository.Table.Where(sa => sa.AbcId == branchId).FirstOrDefault();
            if (shopAbc == null)
            {
                return null;
            }

            var shopId = shopAbc.ShopId;

            return _shopRepository.Table.Where(s => s.Id == shopId).FirstOrDefault();
        }

        public Shop GetShopByName(string name)
        {
            return _shopRepository.Table.Where(s => s.Name == name).FirstOrDefault();
        }

        /*public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddHttpClient("SmsClient", async (serviceProvider, client) =>
            {
                var workContext = serviceProvider.GetRequiredService<IWorkContext>();
                var genericAttributeService = serviceProvider.GetRequiredService<IGenericAttributeService>();

                var currentCustomer = await workContext.GetCurrentCustomerAsync();
                var bearerToken = await genericAttributeService.GetAttributeAsync<string>(currentCustomer, "SmsBearerToken");

                if (string.IsNullOrEmpty(bearerToken) || IsTokenExpired(bearerToken)) // Implement IsTokenExpired
                {
                    bearerToken = await GetBearerTokenAsync();
                    await genericAttributeService.SaveAttributeAsync(currentCustomer, "SmsBearerToken", bearerToken);
                }

                client.BaseAddress = new Uri("https://api.listrak.com/sms");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            });

            return services;
        }*/

        public static async Task<string> GetBearerTokenAsync(IGenericAttributeService genericAttributeService, IWorkContext workContext)
        {
            using var client = new HttpClient();
            var clientId = "YourClientId"; // Retrieve from secure configuration
            var clientSecret = "YourClientSecret"; // Retrieve from secure configuration

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.listrak.com/auth/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "client_credentials" }
                })
            };

            var response = await client.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

            if (tokenResponse != null)
            {
                var token = tokenResponse.AccessToken;
                var expirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                // Save token and expiration time
                var currentCustomer = await workContext.GetCurrentCustomerAsync();
                await genericAttributeService.SaveAttributeAsync(currentCustomer, "SmsBearerToken", token);
                await genericAttributeService.SaveAttributeAsync(currentCustomer, "SmsTokenExpiration", expirationTime.ToString("o"));

                return token;
            }

            throw new Exception("Failed to retrieve token.");
        }

        public static async Task<bool> IsTokenExpiredAsync(IWorkContext workContext, IGenericAttributeService genericAttributeService)
        {
            var currentCustomer = await workContext.GetCurrentCustomerAsync();
            var expirationString = await genericAttributeService.GetAttributeAsync<string>(currentCustomer, "SmsTokenExpiration");

            if (DateTime.TryParse(expirationString, out var expirationTime))
            {
                return DateTime.UtcNow >= expirationTime;
            }

            return true; // Consider the token expired if expiration time is missing.
        }

        Task<string> ICustomShopService.GetBearerTokenAsync(IGenericAttributeService genericAttributeService, IWorkContext workContext)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICustomShopService.IsTokenExpiredAsync(IWorkContext workContext, IGenericAttributeService genericAttributeService)
        {
            throw new NotImplementedException();
        }

        Task<string> ICustomShopService.GetBearerTokenAsync()
        {
            throw new NotImplementedException();
        }

        Task<bool> ICustomShopService.IsTokenExpiredAsync()
        {
            throw new NotImplementedException();
        }

        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}