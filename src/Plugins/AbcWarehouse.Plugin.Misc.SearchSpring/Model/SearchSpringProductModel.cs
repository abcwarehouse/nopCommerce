using System.Collections.Generic;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Models
{
    public class SearchSpringProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Price { get; set; }
        public string ProductUrl { get; set; }
    }

    public class SearchSpringResponse
    {
        public List<SearchSpringProductModel> Results { get; set; }
    }
}
