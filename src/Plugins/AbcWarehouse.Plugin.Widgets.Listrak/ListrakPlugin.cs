using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace AbcWarehouse.Plugin.Widgets.Listrak
{
    public class ListrakPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public ListrakPlugin(
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
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
            await SeedDefaultCredentialsIfMissingAsync();

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
            await SeedDefaultCredentialsIfMissingAsync();
        }

        private async Task UpdateLocales()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                new Dictionary<string, string>
                {
                    [ListrakLocales.MerchantId] = "Merchant ID",
                    [ListrakLocales.MerchantIdHint] = "The merchant ID provided by Listrak.",
                    [ListrakLocales.ClientId] = "Client ID",
                    [ListrakLocales.ClientIdHint] = "The OAuth2 client_id used to authenticate against Listrak's SMS API.",
                    [ListrakLocales.ClientSecret] = "Client Secret",
                    [ListrakLocales.ClientSecretHint] = "The OAuth2 client_secret used to authenticate against Listrak's SMS API.",
                });
        }

        /// <summary>
        /// ClientId/ClientSecret used to be hardcoded in ListrakService. This one-time seed keeps
        /// existing installs working after upgrading to admin-configurable credentials, without
        /// overwriting a value an admin has already set. The seeded values were previously
        /// committed to source control, so they should be rotated in Listrak and re-entered via
        /// Admin > Widgets.Listrak > Configure rather than trusted long-term.
        /// </summary>
        private async Task SeedDefaultCredentialsIfMissingAsync()
        {
            var settings = await _settingService.LoadSettingAsync<ListrakSettings>();
            var changed = false;

            if (string.IsNullOrEmpty(settings.ClientId))
            {
                settings.ClientId = "ao1xkc57sz7t1dw1qawh";
                changed = true;
            }

            if (string.IsNullOrEmpty(settings.ClientSecret))
            {
                settings.ClientSecret = "rDpBSv2PMMrpo2Nso0AAyFqiag1U395bYV4ltx1vhIE";
                changed = true;
            }

            if (changed)
                await _settingService.SaveSettingAsync(settings);
        }
    }
}
