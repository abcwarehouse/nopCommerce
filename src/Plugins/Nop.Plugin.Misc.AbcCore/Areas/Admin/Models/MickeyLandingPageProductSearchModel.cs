using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public partial record MickeyLandingPageProductSearchModel : BaseSearchModel
    {
        public int LandingPageId { get; set; }
        public string LandingPageName { get; set; }
    }
}
