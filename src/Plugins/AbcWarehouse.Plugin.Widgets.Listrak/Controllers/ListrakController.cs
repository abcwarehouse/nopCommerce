using System.Threading.Tasks;
using AbcWarehouse.Plugin.Widgets.Listrak.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Core;

namespace AbcWarehouse.Plugin.Widgets.Listrak.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class ListrakController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        public ListrakController(
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        public async Task<ActionResult> Configure()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<ListrakSettings>(storeScope);

            var model = new ConfigModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                MerchantId = settings.MerchantId,
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                SenderCodeId = settings.SenderCodeId
            };

            if (storeScope > 0)
            {
                model.MerchantId_OverrideForStore =
                    await _settingService.SettingExistsAsync(settings, x => x.MerchantId, storeScope);
                model.ClientId_OverrideForStore =
                    await _settingService.SettingExistsAsync(settings, x => x.ClientId, storeScope);
                model.ClientSecret_OverrideForStore =
                    await _settingService.SettingExistsAsync(settings, x => x.ClientSecret, storeScope);
                model.SenderCodeId_OverrideForStore =
                    await _settingService.SettingExistsAsync(settings, x => x.SenderCodeId, storeScope);
            }

            return View("~/Plugins/Widgets.Listrak/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<ActionResult> Configure(ConfigModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var existingSettings = await _settingService.LoadSettingAsync<ListrakSettings>(storeScope);

            var settings = new ListrakSettings()
            {
                MerchantId = model.MerchantId,
                ClientId = model.ClientId,
                // ClientSecret is a [DataType(DataType.Password)] field, which ASP.NET Core's
                // default editor template always renders blank on page load regardless of the
                // saved value (a deliberate convention - don't echo secrets into HTML). Treat a
                // blank submission as "leave it unchanged" rather than wiping out the real
                // secret every time this form is saved for any other reason.
                ClientSecret = string.IsNullOrEmpty(model.ClientSecret) ? existingSettings.ClientSecret : model.ClientSecret,
                SenderCodeId = model.SenderCodeId
            };

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ClientSecret, model.ClientSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SenderCodeId, model.SenderCodeId_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
