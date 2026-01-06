using System;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Core;
using Nop.Services.Localization;

namespace Nop.Plugin.Widgets.AbcHomeDeliveryStatus
{
    public class HomeDeliveryStatusWidget : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public HomeDeliveryStatusWidget(
            ISettingService settingService,
            IWebHelper webHelper,
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
        }

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone) => typeof(Components.WidgetsAbcHomeDeliveryStatusViewComponent);

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { "topic_page_after_body" });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AbcHomeDeliveryStatus/Configure";
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(AbcHomeDeliveryStatusSettings.Default());

            // Add localization resources
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Widgets.AbcHomeDeliveryStatus.Fields.UseMockResponses", "Use Mock Responses");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Widgets.AbcHomeDeliveryStatus.Fields.UseMockResponses.Hint", "When enabled, the delivery status API will return mock data instead of making real API calls.");

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<AbcHomeDeliveryStatusSettings>();

            // Delete localization resources
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Widgets.AbcHomeDeliveryStatus.Fields.UseMockResponses");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Widgets.AbcHomeDeliveryStatus.Fields.UseMockResponses.Hint");

            await base.UninstallAsync();
        }
    }
}
