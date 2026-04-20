using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public partial record MickeyLandingPageProductModel : BaseNopEntityModel
    {
        public int MappingId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
