using System.Collections.Generic;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.ViewEngines
{
    public class CustomViewEngine : SevenSpikes.Nop.Framework.ViewLocations.ViewLocationsManager
    {
        public CustomViewEngine()
        {
            AddViewLocationFormats(
                new List<string>
                {
                    "~/Plugins/Misc.SearchSpring/Views/{1}/{0}.cshtml",
                    "~/Plugins/Misc.SearchSpring/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.SearchSpring/Views/{0}.cshtml",
                }, true);
        }
    }
}
