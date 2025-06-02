using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public record AbcPromoEditPopupModel : BaseNopEntityModel
    {
        public AbcPromoEditPopupModel()
        {
            AvailableBrands = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.AbcPromo.Fields.IsBrandOverridden")]
        public bool IsBrandOverridden { get; set; }

        public int BrandId { get; set; }

        public IList<SelectListItem> AvailableBrands { get; set; }
    }
}