using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: "SearchSpringBeacon",
                pattern: "searchspring/beacon/events",
                defaults: new { controller = "SearchSpringBeacon", action = "SendEvent" }
            );
        }

        public int Priority => 0;
    }
}
