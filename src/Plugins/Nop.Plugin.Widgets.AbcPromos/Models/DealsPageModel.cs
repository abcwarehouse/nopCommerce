using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Plugin.Misc.AbcPromos.Models
{
    public record DealsPageModel : BasePageableModel
    {
        public IList<ProductOverviewModel> Products { get; set; }
        public int? OrderBy { get; set; }
        public bool AllowProductSorting { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }
        public IList<SelectListItem> AvailablePromoFilters { get; set; }
        public IList<SelectListItem> AvailableCategoryFilters { get; set; }
        public int? SelectedPromoId { get; set; }
        public int? SelectedCategoryId { get; set; }

        public DealsPageModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
            AvailablePromoFilters = new List<SelectListItem>();
            AvailableCategoryFilters = new List<SelectListItem>();
        }
    }
}
