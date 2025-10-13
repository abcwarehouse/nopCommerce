using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;
using System.Web;
using System.Linq;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public class SearchSpringService : ISearchSpringService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://4lt84w.a.searchspring.io";
        private readonly ILogger _logger;
        public RecommendedProduct product = new RecommendedProduct();

        public SearchSpringService(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<SearchResultModel> SearchAsync(string query, string sessionId = null,
                                                         string userId = null, string siteId = "4lt84w",
                                                         int page = 1, Dictionary<string, List<string>> filters = null,
                                                         string sort = null, double? latitude = null, double? longitude = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query must not be null or empty.", nameof(query));

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler);

            var queryParams = new List<string>
            {
                $"q={HttpUtility.UrlEncode(query)}",
                "resultsFormat=json",
                "resultsPerPage=25",
                $"page={page}",
                "redirectResponse=direct"
            };

            if (!string.IsNullOrEmpty(sessionId))
                queryParams.Add($"ss-sessionId={HttpUtility.UrlEncode(sessionId)}");

            if (!string.IsNullOrEmpty(userId))
                queryParams.Add($"ss-shopperId={HttpUtility.UrlEncode(userId)}");

            await _logger.InsertLogAsync(LogLevel.Information, $"[SearchSpring] Shopper Id Check: {userId}");

            if (!string.IsNullOrEmpty(siteId))
                queryParams.Add($"siteId={HttpUtility.UrlEncode(siteId)}");

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    foreach (var value in filter.Value)
                    {
                        queryParams.Add($"filter.{HttpUtility.UrlEncode(filter.Key)}={HttpUtility.UrlEncode(value)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var parts = sort.Contains(":") ? sort.Split(':') :
                            sort.Contains("_") ? sort.Split('_') : null;

                if (parts?.Length == 2)
                {
                    var field = parts[0];
                    var direction = parts[1];
                    queryParams.Add($"sort.{HttpUtility.UrlEncode(field)}={HttpUtility.UrlEncode(direction)}");
                }
                else
                {
                    queryParams.Add("sort.relevance=desc");
                }
            }
            else
            {
                queryParams.Add("sort.relevance=desc");
            }

            var url = $"{_baseUrl}/api/search/search.json?{string.Join("&", queryParams)}";

            var response = await client.GetAsync(url);

            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
            {
                if (response.Headers.Location != null)
                {
                    var redirectUrl = response.Headers.Location.ToString();
                    await _logger.InsertLogAsync(LogLevel.Information, $"[SearchSpring] Redirect detected: {redirectUrl}");
                    return new SearchResultModel
                    {
                        RedirectResponse = redirectUrl
                    };
                }
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _logger.InsertLogAsync(LogLevel.Error,
                    $"[SearchSpring] API Error {response.StatusCode}: {json}");
                throw new Exception($"Searchspring returned error {response.StatusCode}: {json}\nURL: {url}");
            }

            try
            {
                var productList = new List<SearchSpringProductModel>();
                int currentPage = 1, pageSize = 24, totalResults = 0;
                var facets = new Dictionary<string, FacetDetail>();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Legacy redirect parsing
                if (root.TryGetProperty("redirect", out var redirectProp) &&
                    redirectProp.TryGetProperty("url", out var redirectUrlProp) &&
                    redirectUrlProp.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrEmpty(redirectUrlProp.GetString()))
                {
                    var redirectUrl = redirectUrlProp.GetString();
                    return new SearchResultModel
                    {
                        RedirectResponse = redirectUrl
                    };
                }

                // Parse results
                if (root.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in resultsElement.EnumerateArray())
                    {
                        var model = new SearchSpringProductModel
                        {
                            Id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : "",
                            Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "",
                            ProductUrl = item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : "",
                            ImageUrl = item.TryGetProperty("imageUrl", out var imgProp) ? imgProp.GetString() : "",
                            Price = item.TryGetProperty("price", out var priceProp) ? priceProp.GetString() : "",
                            Brand = item.TryGetProperty("brand", out var brandProp) ? brandProp.GetString() : "",
                            Category = item.TryGetProperty("category", out var catProp) ? catProp.GetString() : "",
                            ItemNumber = item.TryGetProperty("item_number", out var itemNumProp) ? itemNumProp.GetString() : "",
                            RetailPrice = item.TryGetProperty("retail_price", out var retailPriceProp) ? retailPriceProp.GetString() : "",
                            Sku = item.TryGetProperty("sku", out var skuProp) ? skuProp.GetString() : ""
                        };
                        productList.Add(model);
                    }
                }

                // Parse pagination
                if (root.TryGetProperty("pagination", out var pagination) && pagination.ValueKind == JsonValueKind.Object)
                {
                    currentPage = pagination.TryGetProperty("currentPage", out var pageProp) ? pageProp.GetInt32() : 1;
                    pageSize = pagination.TryGetProperty("pageSize", out var sizeProp) ? sizeProp.GetInt32() : 25;
                    totalResults = pagination.TryGetProperty("totalResults", out var totalProp) ? totalProp.GetInt32() : 0;
                }

                // Parse facets
                if (root.TryGetProperty("facets", out var facetsProp) && facetsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var facet in facetsProp.EnumerateArray())
                    {
                        var field = facet.TryGetProperty("field", out var fieldProp) ? fieldProp.GetString() : "";

                        var detail = new FacetDetail
                        {
                            Field = field,
                            Label = facet.TryGetProperty("label", out var labelProp) ? labelProp.GetString() : "",
                            Multiple = facet.TryGetProperty("multiple", out var multipleProp) ? multipleProp.GetString() : "",
                            Collapse = facet.TryGetProperty("collapse", out var collapseProp) && collapseProp.GetInt32() == 1
                        };

                        if (facet.TryGetProperty("values", out var valuesProp) && valuesProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var val in valuesProp.EnumerateArray())
                            {
                                detail.Values.Add(new FacetValue
                                {
                                    Value = val.TryGetProperty("value", out var v) ? v.GetString() : "",
                                    Label = val.TryGetProperty("label", out var l) ? l.GetString() : "",
                                    Count = val.TryGetProperty("count", out var c) ? c.GetInt32() : 0
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(field))
                            facets[field] = detail;
                    }
                }

                // Parse sort options
                var sortOptions = new List<SortOption>();
                if (root.TryGetProperty("sorting", out var sortingProp) &&
                    sortingProp.TryGetProperty("options", out var optionsProp) &&
                    optionsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sortOption in optionsProp.EnumerateArray())
                    {
                        sortOptions.Add(new SortOption
                        {
                            Field = sortOption.TryGetProperty("field", out var fieldProp) ? fieldProp.GetString() : "",
                            Direction = sortOption.TryGetProperty("direction", out var dirProp) ? dirProp.GetString() : "",
                            Label = sortOption.TryGetProperty("label", out var labelProp) ? labelProp.GetString() : ""
                        });
                    }
                }

                var bannersByPosition = new Dictionary<string, List<string>>();

                if (root.TryGetProperty("merchandising", out var merchProp))
                {

                    var merchJson = merchProp.GetRawText();

                    if (merchProp.TryGetProperty("content", out var contentProp))
                    {

                        if (contentProp.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var property in contentProp.EnumerateObject())
                            {
                                await _logger.InsertLogAsync(LogLevel.Information,
                                    $"[SearchSpring] Processing content position: {property.Name}");

                                if (property.Value.ValueKind == JsonValueKind.Array)
                                {
                                    var banners = property.Value.EnumerateArray()
                                        .Where(b => b.ValueKind == JsonValueKind.String)
                                        .Select(b => b.GetString())
                                        .Where(html => !string.IsNullOrEmpty(html))
                                        .ToList();

                                    if (banners.Any())
                                    {
                                        bannersByPosition[property.Name] = banners;
                                        foreach (var banner in banners)
                                        {
                                            var preview = banner.Length > 100 ? banner.Substring(0, 100) + "..." : banner;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Warning,
                            "[SearchSpring] No 'content' property found in merchandising block");
                    }

                    // Parse triggered campaigns
                    if (merchProp.TryGetProperty("triggeredCampaigns", out var campaignsProp) &&
                        campaignsProp.ValueKind == JsonValueKind.Array)
                    {
                        var campaignCount = campaignsProp.GetArrayLength();
                        await _logger.InsertLogAsync(LogLevel.Information,
                            $"[SearchSpring] Found {campaignCount} triggered campaign(s)");

                        foreach (var campaign in campaignsProp.EnumerateArray())
                        {
                            var campaignJson = campaign.GetRawText();
                            await _logger.InsertLogAsync(LogLevel.Information,
                                $"[SearchSpring] Campaign JSON: {campaignJson}");

                            var placement = campaign.TryGetProperty("placement", out var placementProp)
                                ? placementProp.GetString()
                                : "header";

                            var html = campaign.TryGetProperty("html", out var htmlProp)
                                ? htmlProp.GetString()
                                : null;

                            await _logger.InsertLogAsync(LogLevel.Information,
                                $"[SearchSpring] Campaign placement: {placement}, Has HTML: {!string.IsNullOrWhiteSpace(html)}");

                            if (!string.IsNullOrWhiteSpace(placement) && !string.IsNullOrWhiteSpace(html))
                            {
                                if (!bannersByPosition.ContainsKey(placement))
                                    bannersByPosition[placement] = new List<string>();

                                bannersByPosition[placement].Add(html.Trim());
                            }
                        }
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Warning,
                            "[SearchSpring] No 'triggeredCampaigns' array found in merchandising block");
                    }
                }
                else
                {
                    await _logger.InsertLogAsync(LogLevel.Warning,
                        "[SearchSpring] NO merchandising block found in API response");
                }

                return new SearchResultModel
                {
                    Results = productList,
                    PageNumber = currentPage,
                    PageSize = pageSize,
                    TotalResults = totalResults,
                    Facets = facets,
                    SortOptions = sortOptions,
                    BannersByPosition = bannersByPosition,
                    Query = query
                };
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error,
                    $"[SearchSpring] JSON Deserialization failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception("Failed to parse Searchspring response.", ex);
            }
        }

        public async Task<RecommendationsResultModel> GetRecommendationsAsync(RecommendationsRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Tags))
                throw new ArgumentException("Tags parameter is required for recommendations", nameof(request.Tags));

            var client = _httpClientFactory.CreateClient();
            
            // Build the URL
            var siteId = request.SiteId ?? "4lt84w";
            var queryParams = new List<string>
            {
                $"tags={HttpUtility.UrlEncode(request.Tags)}"
            };

            // Add optional parameters
            if (!string.IsNullOrWhiteSpace(request.Products))
                queryParams.Add($"products={HttpUtility.UrlEncode(request.Products)}");
            
            if (!string.IsNullOrWhiteSpace(request.BlockedItems))
                queryParams.Add($"blockedItems={HttpUtility.UrlEncode(request.BlockedItems)}");
            
            if (!string.IsNullOrWhiteSpace(request.Categories))
                queryParams.Add($"categories={HttpUtility.UrlEncode(request.Categories)}");
            
            if (!string.IsNullOrWhiteSpace(request.Brands))
                queryParams.Add($"brands={HttpUtility.UrlEncode(request.Brands)}");
            
            if (!string.IsNullOrWhiteSpace(request.Shopper))
                queryParams.Add($"shopper={HttpUtility.UrlEncode(request.Shopper)}");
            
            // Add session ID for personalization
            if (!string.IsNullOrWhiteSpace(request.SessionId))
                queryParams.Add($"sessionId={HttpUtility.UrlEncode(request.SessionId)}");
            
            if (!string.IsNullOrWhiteSpace(request.Cart))
                queryParams.Add($"cart={HttpUtility.UrlEncode(request.Cart)}");
            
            if (!string.IsNullOrWhiteSpace(request.LastViewed))
                queryParams.Add($"lastViewed={HttpUtility.UrlEncode(request.LastViewed)}");
            
            if (!string.IsNullOrWhiteSpace(request.Limits))
                queryParams.Add($"limits={HttpUtility.UrlEncode(request.Limits)}");

            // Add filters
            if (request.Filters != null)
            {
                foreach (var filter in request.Filters)
                {
                    queryParams.Add($"filter.{HttpUtility.UrlEncode(filter.Key)}={HttpUtility.UrlEncode(filter.Value)}");
                }
            }

            var url = $"https://{siteId}.a.searchspring.io/boost/{siteId}/recommend?{string.Join("&", queryParams)}";

            await _logger.InsertLogAsync(LogLevel.Information, 
                $"[SearchSpring Recommendations] Request URL: {url}");

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            await _logger.InsertLogAsync(LogLevel.Information, 
                $"[SearchSpring Recommendations Service] Raw JSON Response: {json}");

            if (!response.IsSuccessStatusCode)
            {
                await _logger.InsertLogAsync(LogLevel.Error, 
                    $"[SearchSpring Recommendations] API Error {response.StatusCode}: {json}");
                throw new Exception($"SearchSpring Recommendations API returned error {response.StatusCode}: {json}");
            }

            try
            {
                var result = new RecommendationsResultModel();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var profileElement in root.EnumerateArray())
                    {
                        var profile = new RecommendationProfile
                        {
                            Tag = profileElement.TryGetProperty("tag", out var tagProp) ? tagProp.GetString() : "",
                            Display = profileElement.TryGetProperty("display", out var displayProp) ? displayProp.GetString() : ""
                        };

                        if (profileElement.TryGetProperty("results", out var resultsElement))
                        {
                            foreach (var productElement in resultsElement.EnumerateArray())
                            {
                                var product = new RecommendedProduct
                                {
                                    Id = productElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : ""
                                };

                                if (productElement.TryGetProperty("mappings", out var mappingsProp) &&
                                    mappingsProp.TryGetProperty("core", out var coreProp))
                                {
                                    product.Sku = coreProp.TryGetProperty("sku", out var skuProp) ? skuProp.GetString() : "";
                                    product.Name = coreProp.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";
                                    product.Url = coreProp.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : "";
                                    product.ImageUrl = coreProp.TryGetProperty("imageUrl", out var imgProp) ? imgProp.GetString() : "";
                                    product.Price = coreProp.TryGetProperty("price", out var priceProp) ? priceProp.GetDecimal().ToString("0.##") : "";
                                    product.RetailPrice = coreProp.TryGetProperty("msrp", out var retailProp) ? retailProp.GetDecimal().ToString("0.##") : "";
                                    product.Brand = coreProp.TryGetProperty("brand", out var brandProp) ? brandProp.GetString() : "";
                                }

                                // Optional attributes
                                if (productElement.TryGetProperty("attributes", out var attrProp) &&
                                    attrProp.TryGetProperty("category", out var catProp))
                                {
                                    product.Category = catProp.GetString();
                                }

                                if (productElement.TryGetProperty("in_stock", out var stockProp))
                                {
                                    product.InStock = stockProp.GetInt32();
                                }

                                profile.Results.Add(product);
                            }
                        }

                        result.Profiles.Add(profile);
                    }
                }

                await _logger.InsertLogAsync(LogLevel.Information,
                    $"[SearchSpring Recommendations] Parsed {result.Profiles.Count} profiles successfully.");

                return result;
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error,
                    $"[SearchSpring Recommendations] JSON Parsing failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception("Failed to parse SearchSpring Recommendations response.", ex);
            }
        }

        private bool IsKnownProperty(string propertyName)
        {
            var knownProps = new[] 
            { 
                "id", "sku", "name", "url", "imageUrl", "price", 
                "retail_price", "brand", "category", "item_number", "in_stock" 
            };
            return knownProps.Contains(propertyName.ToLower());
        }

    }
}