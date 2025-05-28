using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public record AbcPromoEditPopupModel : BaseNopEntityModel
    {
        public bool OverrideBrand { get; set; }
    }
}