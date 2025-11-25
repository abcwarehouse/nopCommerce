using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.MickeySalePromo.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.MickeySalePromo.Components
{
    [ViewComponent(Name = "WidgetsMickeySalePromo")]
    public class WidgetsMickeySalePromoViewComponent : NopViewComponent
    {
        private readonly MickeySalePromoSettings _settings;

        public WidgetsMickeySalePromoViewComponent(MickeySalePromoSettings settings)
        {
            _settings = settings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {
            // Check if the widget should be displayed based on conditions
            if (!ShouldDisplay())
            {
                return Content("");
            }

            // Pass settings to the view
            return View("~/Plugins/Widgets.MickeySalePromo/Views/PublicInfo.cshtml", _settings);
        }

        private bool ShouldDisplay()
        {
            // If ShowOnAllPages is enabled, always display
            if (_settings.ShowOnAllPages)
            {
                return true;
            }

            // Get route data from the current request
            var routeData = HttpContext.Request.RouteValues;

            // Check if we're on a category page
            if (routeData.ContainsKey("categoryid") || routeData.ContainsKey("controller") && routeData["controller"]?.ToString()?.Equals("Catalog", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                if (!string.IsNullOrEmpty(_settings.CategoryIds))
                {
                    var allowedCategoryIds = _settings.CategoryIds
                        .Split(',')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (allowedCategoryIds.Any() && routeData.ContainsKey("categoryid"))
                    {
                        var currentCategoryId = routeData["categoryid"]?.ToString();
                        if (allowedCategoryIds.Contains(currentCategoryId))
                        {
                            return true;
                        }
                    }
                }
            }

            // Check if we're on a topic page
            if (routeData.ContainsKey("systemname") || (routeData.ContainsKey("controller") && routeData["controller"]?.ToString()?.Equals("Topic", System.StringComparison.OrdinalIgnoreCase) == true))
            {
                if (!string.IsNullOrEmpty(_settings.TopicSystemNames))
                {
                    var allowedTopicNames = _settings.TopicSystemNames
                        .Split(',')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (allowedTopicNames.Any() && routeData.ContainsKey("systemname"))
                    {
                        var currentTopicName = routeData["systemname"]?.ToString();
                        if (allowedTopicNames.Any(t => t.Equals(currentTopicName, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                }
            }

            // If no conditions are set, show on all pages
            if (string.IsNullOrEmpty(_settings.CategoryIds) && string.IsNullOrEmpty(_settings.TopicSystemNames))
            {
                return true;
            }

            // Don't display if conditions don't match
            return false;
        }
    }
}
