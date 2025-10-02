using System.Collections.Generic;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Models
{
    /// <summary>
    /// Request model for SearchSpring Recommendations API
    /// </summary>
    public class RecommendationsRequestModel
    {
        /// <summary>
        /// Required: Comma-separated list of profile IDs/tags
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Comma-separated list of product IDs or SKUs being viewed
        /// Required for cross-sell or similar recommendation profiles
        /// </summary>
        public string Products { get; set; }

        /// <summary>
        /// Comma-separated list of product IDs or SKUs to exclude from results
        /// </summary>
        public string BlockedItems { get; set; }

        /// <summary>
        /// Comma-separated list of category IDs
        /// Required for Category Trending profiles
        /// </summary>
        public string Categories { get; set; }

        /// <summary>
        /// Comma-separated list of brand names
        /// Required for Brand Trending profiles
        /// </summary>
        public string Brands { get; set; }

        /// <summary>
        /// ID of current logged-in shopper
        /// Required for personalization based on shopper history
        /// </summary>
        public string Shopper { get; set; }

        /// <summary>
        /// Comma-separated list of product IDs or SKUs in the shopper's cart
        /// Required for cart cross-sell
        /// </summary>
        public string Cart { get; set; }

        /// <summary>
        /// Comma-separated list of recently viewed product SKUs
        /// Most recent should be listed first
        /// </summary>
        public string LastViewed { get; set; }

        /// <summary>
        /// Comma-separated list of integers for max results per profile
        /// One integer per profile in the Tags parameter
        /// Default is 20 if not specified
        /// </summary>
        public string Limits { get; set; }

        /// <summary>
        /// Filters to apply to recommendations
        /// Key: field name, Value: filter value
        /// Use .low/.high suffix for range filters (e.g., "price.low", "price.high")
        /// </summary>
        public Dictionary<string, string> Filters { get; set; }

        /// <summary>
        /// SearchSpring site ID
        /// </summary>
        public string SiteId { get; set; } = "4lt84w";
    }

    /// <summary>
    /// Response model from SearchSpring Recommendations API
    /// </summary>
    public class RecommendationsResultModel
    {
        /// <summary>
        /// List of recommendation profiles returned
        /// </summary>
        public List<RecommendationProfile> Profiles { get; set; } = new List<RecommendationProfile>();
    }

    /// <summary>
    /// Individual recommendation profile with its results
    /// </summary>
    public class RecommendationProfile
    {
        /// <summary>
        /// Profile tag/ID
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Display name for the profile (e.g., "You May Also Like")
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// List of recommended products
        /// </summary>
        public List<RecommendedProduct> Results { get; set; } = new List<RecommendedProduct>();
    }

    /// <summary>
    /// Recommended product data
    /// </summary>
    public class RecommendedProduct
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Product SKU
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Product URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Current price
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Original retail price (MSRP)
        /// </summary>
        public string RetailPrice { get; set; }

        /// <summary>
        /// Brand name
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// ABC Warehouse item number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Stock status (1 = in stock, 0 = out of stock)
        /// </summary>
        public int? InStock { get; set; }

        /// <summary>
        /// Additional fields not mapped to specific properties
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}