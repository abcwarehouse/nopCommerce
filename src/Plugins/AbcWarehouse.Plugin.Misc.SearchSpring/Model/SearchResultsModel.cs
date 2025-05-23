using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Models
{
    public class SearchResultModel
    {
        [JsonPropertyName("results")]
        public List<SearchProductModel> Results { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationModel Pagination { get; set; }
    }

    public class SearchProductModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class PaginationModel
    {
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
    }
}
