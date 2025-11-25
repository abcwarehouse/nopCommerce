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
            // Check if topic filtering is enabled (TopicId > 0)
            if (_settings.TopicId > 0)
            {
                // Get the current topic ID from route data
                var routeData = HttpContext.Request.RouteValues;

                // Check if we're on a topic page
                if (routeData.ContainsKey("controller") &&
                    routeData["controller"]?.ToString()?.Equals("Topic", System.StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Get the topicId from route data
                    if (routeData.ContainsKey("topicId"))
                    {
                        var currentTopicId = routeData["topicId"]?.ToString();

                        // Only show if the current topic matches the selected topic
                        if (currentTopicId != _settings.TopicId.ToString())
                        {
                            return Content("");
                        }
                    }
                    else
                    {
                        // We're on a topic page but couldn't get the ID, don't show
                        return Content("");
                    }
                }
                else
                {
                    // We're not on a topic page, don't show
                    return Content("");
                }
            }

            // Pass settings to the view
            return View("~/Plugins/Widgets.MickeySalePromo/Views/PublicInfo.cshtml", _settings);
        }
    }
}
