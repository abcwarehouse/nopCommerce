using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Models
{
    public class SearchResultModel
    {
        public List<SearchSpringProductModel> Results { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);

        public string Query { get; set; }
        public Dictionary<string, FacetDetail> Facets { get; set; } = new();
        public List<SortOption> SortOptions { get; set; } = new();
    }

    public class FacetDetail
    {
        public string Multiple { get; set; }
        public string Display { get; set; }
        public string Label { get; set; }
        public bool Collapsed { get; set; }

        public List<FacetValue> Values { get; set; } = new();
    }

    public class FacetValue
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class SortOption
    {
        public string Type { get; set; }
        public string Field { get; set; }
        public string Direction { get; set; }
        public string Label { get; set; }
    }

}
