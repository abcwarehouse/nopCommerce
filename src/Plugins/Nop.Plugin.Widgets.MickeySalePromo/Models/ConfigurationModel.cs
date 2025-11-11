using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MickeySalePromo.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerDesktopUrl")]
        public string TopBannerDesktopUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerMobileUrl")]
        public string TopBannerMobileUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.LeftBannerUrl")]
        public string LeftBannerUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.RightBannerUrl")]
        public string RightBannerUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.DisclaimerText")]
        public string DisclaimerText { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.WidgetZone")]
        public string WidgetZone { get; set; }
    }
}
