using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public partial record MickeyLandingPageModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }
        public string DateRange { get; set; }
        public int ProductCount { get; set; }
    }
}
