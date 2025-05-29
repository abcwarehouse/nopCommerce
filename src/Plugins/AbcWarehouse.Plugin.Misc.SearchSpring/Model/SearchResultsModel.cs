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
    }

    public class PaginationModel
    {
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
