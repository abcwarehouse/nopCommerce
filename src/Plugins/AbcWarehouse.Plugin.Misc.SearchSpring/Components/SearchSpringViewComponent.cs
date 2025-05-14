using Microsoft.AspNetCore.Mvc;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Components
{
    public class SearchSpringViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Shared/Components/SearchSpring/Default.cshtml");
        }
    }
}
