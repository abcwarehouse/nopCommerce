using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace AbcWarehouse.Plugin.Widgets.MarketingPixels.Components
{
    /// <summary>
    /// Outputs a marketing pixel only on the pages it is meant for:
    /// - Home page                 -> home pixel (conf + view)
    /// - /save-more-event (exact)  -> save-more pixel (conf)
    /// Every other page renders nothing.
    /// </summary>
    public class MarketingPixelsViewComponent : NopViewComponent
    {
        private const string SaveMorePath = "/save-more-event";

        public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            if (IsHomePage())
                return Task.FromResult<IViewComponentResult>(View("~/Plugins/Widgets.MarketingPixels/Views/Home.cshtml"));

            if (IsSaveMorePage())
                return Task.FromResult<IViewComponentResult>(View("~/Plugins/Widgets.MarketingPixels/Views/SaveMoreEvent.cshtml"));

            return Task.FromResult<IViewComponentResult>(Content(string.Empty));
        }

        private bool IsHomePage()
        {
            var routeData = Url.ActionContext.RouteData;
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();
            return string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSaveMorePage()
        {
            var path = Url.ActionContext.HttpContext.Request.Path.Value ?? string.Empty;
            path = path.TrimEnd('/');
            return string.Equals(path, SaveMorePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
