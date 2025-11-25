using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Widgets.MickeySalePromo.Models;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.MickeySalePromo
{
    public class MickeySalePromoPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly MickeySalePromoSettings _settings;

        public MickeySalePromoPlugin(
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ISettingService settingService,
            MickeySalePromoSettings settings)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _settingService = settingService;
            _settings = settings;
        }

        public bool HideInWidgetList => false;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsMickeySalePromo";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            // Return the configured widget zone from settings
            var widgetZones = new List<string>();

            if (!string.IsNullOrEmpty(_settings.WidgetZone))
            {
                widgetZones.Add(_settings.WidgetZone);
            }

            return Task.FromResult<IList<string>>(widgetZones);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MickeySalePromo/Configure";
        }

        public override async Task InstallAsync()
        {
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.MickeySalePromo.TopBannerDesktopUrl"] = "Top Banner Desktop URL",
                ["Plugins.Widgets.MickeySalePromo.TopBannerDesktopUrl.Hint"] = "URL for the desktop top banner image",
                ["Plugins.Widgets.MickeySalePromo.TopBannerMobileUrl"] = "Top Banner Mobile URL",
                ["Plugins.Widgets.MickeySalePromo.TopBannerMobileUrl.Hint"] = "URL for the mobile top banner image",
                ["Plugins.Widgets.MickeySalePromo.LeftBannerUrl"] = "Left Feature Banner URL",
                ["Plugins.Widgets.MickeySalePromo.LeftBannerUrl.Hint"] = "URL for the left feature banner image",
                ["Plugins.Widgets.MickeySalePromo.RightBannerUrl"] = "Right Feature Banner URL",
                ["Plugins.Widgets.MickeySalePromo.RightBannerUrl.Hint"] = "URL for the right feature banner image",
                ["Plugins.Widgets.MickeySalePromo.DisclaimerText"] = "Disclaimer Text",
                ["Plugins.Widgets.MickeySalePromo.DisclaimerText.Hint"] = "Disclaimer text to display at the bottom of the page",
                ["Plugins.Widgets.MickeySalePromo.WidgetZone"] = "Widget Zone",
                ["Plugins.Widgets.MickeySalePromo.WidgetZone.Hint"] = "The widget zone where this promotional page will be displayed"
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.MickeySalePromo");
            await _settingService.DeleteSettingAsync<Models.MickeySalePromoSettings>();

            await base.UninstallAsync();
        }
    }
}
