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

            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapyShare",
                pattern: "deal-therapy/share/{productKey}",
                defaults: new { controller = "DealTherapy", action = "Share" }
            );

            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapyShareImage",
                pattern: "deal-therapy/share-image/{productKey}",
                defaults: new { controller = "DealTherapy", action = "ShareImage" }
            );

            endpointRouteBuilder.MapControllerRoute(
                name: "DealTherapyAdminSubmissions",
                pattern: "Admin/DealTherapy/Submissions",
                defaults: new { controller = "DealTherapy", action = "Submissions", area = "Admin" }
            );
        }

        public int Priority => 0;
    }
}
