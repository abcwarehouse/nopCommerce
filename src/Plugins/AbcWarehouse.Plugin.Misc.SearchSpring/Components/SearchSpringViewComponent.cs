using Microsoft.AspNetCore.Mvc;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Components
{
    public class SearchSpringViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
