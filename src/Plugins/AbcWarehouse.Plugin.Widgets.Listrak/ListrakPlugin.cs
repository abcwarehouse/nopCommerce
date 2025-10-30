﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace AbcWarehouse.Plugin.Widgets.Listrak
{
    public class ListrakPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public ListrakPlugin(
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public bool HideInWidgetList => false;

        public System.Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Components.ListrakViewComponent);
        }

        public System.Threading.Tasks.Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.BodyEndHtmlTagBefore,
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return
                $"{_webHelper.GetStoreLocation()}Admin/Listrak/Configure";
        }

        public override async Task InstallAsync()
        {
            await UpdateLocales();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync(ListrakLocales.Base);

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
                    [ListrakLocales.MerchantId] = "Merchant ID",
                    [ListrakLocales.MerchantIdHint] = "The merchant ID provided by Listrak.",
                });
        }
    }
}
