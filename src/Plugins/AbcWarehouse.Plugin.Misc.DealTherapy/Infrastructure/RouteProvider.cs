using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapy",
                pattern: "deal-therapy",
                defaults: new { controller = "DealTherapy", action = "Index" }
            );

            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapySubmit",
                pattern: "deal-therapy/submit",
                defaults: new { controller = "DealTherapy", action = "Submit" }
            );

            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapyResult",
                pattern: "deal-therapy/result",
                defaults: new { controller = "DealTherapy", action = "Result" }
            );
        }

        public int Priority => 0;
    }
}
