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
            // Pass settings to the view
            return View("~/Plugins/Widgets.MickeySalePromo/Views/PublicInfo.cshtml", _settings);
        }
    }
}
