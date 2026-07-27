using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AbcEventSurveys.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Public survey page, e.g. https://www.abcwarehouse.com/survey/pistons-tailgate-2026
            // Handles both the GET (display the form) and POST (submit the entry) - disambiguated
            // by [HttpGet]/[HttpPost] on the two SurveyController.Index overloads.
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.AbcEventSurveys.Survey",
                "survey/{code}",
                new { controller = "Survey", action = "Index" });
        }
    }
}
