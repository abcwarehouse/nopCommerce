using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbcCore.Models
{
    public class ABCProductDetailsModel
    {
        public int ProductId { get; set; }

        [NopResourceDisplayName(CoreLocales.PLPDescription)]
        public string PLPDescription { get; set; }

        // Mickey Landing Page management
        public IList<MickeyLandingPageAssignment> CurrentLandingPages { get; set; }
            = new List<MickeyLandingPageAssignment>();
        public IList<SelectListItem> AvailableLandingPages { get; set; }
            = new List<SelectListItem>();
    }

    public class MickeyLandingPageAssignment
    {
        public int MappingId { get; set; }
        public int LandingPageId { get; set; }
        public string Name { get; set; }
        public string DateRange { get; set; }
        public bool IsActive { get; set; }
    }
}
