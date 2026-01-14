using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Factories;
using Nop.Plugin.Misc.AbcCore.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.AbcCore.Components
{
    public class CartSlideoutPickupInStoreViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(List<ProductStock> productStock)
        {
            return View("~/Plugins/Misc.AbcCore/Views/CartSlideout/_PickupInStore.cshtml", productStock);
        }
    }
}
