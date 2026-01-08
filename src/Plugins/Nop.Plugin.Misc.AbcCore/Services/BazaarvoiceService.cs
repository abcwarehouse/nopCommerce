using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public class BazaarvoiceService : IBazaarvoiceService
    {
        private readonly string _apiPasskey;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ILogger _logger;
        private readonly bool _isConfigured;

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public BazaarvoiceService(
            CoreSettings coreSettings,
            IStaticCacheManager staticCacheManager,
            ILogger logger)
        {
            _apiPasskey = coreSettings.BazaarvoiceApiPasskey;
            _staticCacheManager = staticCacheManager;
            _logger = logger;
            _isConfigured = !string.IsNullOrWhiteSpace(_apiPasskey);
        }

        public async Task<BazaarvoiceRating> GetProductRatingAsync(int productId)
        {
            if (!_isConfigured)
            {
                return new BazaarvoiceRating();
            }

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                new CacheKey("AbcWarehouse.bazaarvoice.rating.{0}"),
                productId);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await FetchRatingFromApiAsync(productId);
            });
        }

        private async Task<BazaarvoiceRating> FetchRatingFromApiAsync(int productId)
        {
            try
            {
                var url = $"https://api.bazaarvoice.com/data/statistics.json" +
                          $"?apiversion=5.4" +
                          $"&passkey={_apiPasskey}" +
                          $"&filter=productid:eq:{productId}" +
                          $"&stats=reviews";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await _logger.WarningAsync($"Bazaarvoice API returned {response.StatusCode} for product {productId}");
                    return new BazaarvoiceRating();
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BazaarvoiceApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Results != null && result.Results.Length > 0)
                {
                    var stats = result.Results[0]?.ProductStatistics?.ReviewStatistics;
                    if (stats != null)
                    {
                        return new BazaarvoiceRating
                        {
                            AverageRating = stats.AverageOverallRating,
                            ReviewCount = stats.TotalReviewCount
                        };
                    }
                }

                return new BazaarvoiceRating();
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync($"Error fetching Bazaarvoice rating for product {productId}: {ex.Message}", ex);
                return new BazaarvoiceRating();
            }
        }

        // API response models
        private class BazaarvoiceApiResponse
        {
            public BazaarvoiceResult[] Results { get; set; }
        }

        private class BazaarvoiceResult
        {
            public BazaarvoiceProductStatistics ProductStatistics { get; set; }
        }

        private class BazaarvoiceProductStatistics
        {
            public BazaarvoiceReviewStatistics ReviewStatistics { get; set; }
        }

        private class BazaarvoiceReviewStatistics
        {
            public decimal AverageOverallRating { get; set; }
            public int TotalReviewCount { get; set; }
        }
    }
}
