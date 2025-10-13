﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Misc.AbcCore.Infrastructure;

namespace AbcWarehouse.Plugin.Widgets.GA4
{
    public class GA4Plugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public GA4Plugin(
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public bool HideInWidgetList => false;

        public System.Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Components.GA4ViewComponent);
        }

        public System.Threading.Tasks.Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.CheckoutShippingAddressBottom,
                PublicWidgetZones.HeadHtmlTag,
                PublicWidgetZones.ProductDetailsAddInfo,
                CustomPublicWidgetZones.ProductBoxAfter
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return
                $"{_webHelper.GetStoreLocation()}Admin/GA4/Configure";
        }

        public override async Task InstallAsync()
        {
            await UpdateLocales();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync(GA4Locales.Base);

            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string oldVersion, string currentVersion)
        {
            await UpdateLocales();
        }

        private async Task UpdateLocales()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                new Dictionary<string, string>
                {
                    [GA4Locales.GoogleTag] = "Google Tag",
                    [GA4Locales.GoogleTagHint] = "Enter your Google Tag here (should start with G-).",
                    [GA4Locales.IsDebugMode] = "Is Debug View",
                    [GA4Locales.IsDebugModeHint] = "Turn on DebugView within Analytics account.",
                });
        }
    }
}
