using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace AbcWarehouse.Plugin.Misc.StorepointStoreLocator.Controllers
{
    public class StoreLocatorController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Plugins/Misc.StorepointStoreLocator/Views/Index.cshtml");
        }
    }
}
