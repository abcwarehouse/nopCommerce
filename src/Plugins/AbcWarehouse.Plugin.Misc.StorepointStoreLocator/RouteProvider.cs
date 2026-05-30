using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace AbcWarehouse.Plugin.Misc.StorepointStoreLocator
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: "StorepointStoreLocator",
                pattern: "store-locator",
                defaults: new { controller = "StoreLocator", action = "Index" });
        }
    }
}
