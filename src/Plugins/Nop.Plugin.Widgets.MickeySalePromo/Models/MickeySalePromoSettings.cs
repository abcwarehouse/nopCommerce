using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.MickeySalePromo.Models
{
    public class MickeySalePromoSettings : ISettings
    {
        public string TopBannerDesktopUrl { get; set; }
        public string TopBannerMobileUrl { get; set; }
        public string LeftBannerUrl { get; set; }
        public string RightBannerUrl { get; set; }
        public string DisclaimerText { get; set; }
        public string WidgetZone { get; set; }

        internal ConfigurationModel ToModel()
        {
            return new ConfigurationModel
            {
                TopBannerDesktopUrl = TopBannerDesktopUrl,
                TopBannerMobileUrl = TopBannerMobileUrl,
                LeftBannerUrl = LeftBannerUrl,
                RightBannerUrl = RightBannerUrl,
                DisclaimerText = DisclaimerText,
                WidgetZone = WidgetZone
            };
        }

        internal static MickeySalePromoSettings FromModel(ConfigurationModel model)
        {
            return new MickeySalePromoSettings
            {
                TopBannerDesktopUrl = model.TopBannerDesktopUrl,
                TopBannerMobileUrl = model.TopBannerMobileUrl,
                LeftBannerUrl = model.LeftBannerUrl,
                RightBannerUrl = model.RightBannerUrl,
                DisclaimerText = model.DisclaimerText,
                WidgetZone = model.WidgetZone
            };
        }
    }
}
