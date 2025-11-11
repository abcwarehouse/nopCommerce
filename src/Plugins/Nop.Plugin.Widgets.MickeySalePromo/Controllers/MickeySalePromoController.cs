using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.MickeySalePromo.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.MickeySalePromo.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class MickeySalePromoController : BasePluginController
    {
        private readonly MickeySalePromoSettings _settings;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public MickeySalePromoController(
            MickeySalePromoSettings settings,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISettingService settingService)
        {
            _settings = settings;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        public IActionResult Configure()
        {
            return View("~/Plugins/Widgets.MickeySalePromo/Views/Configure.cshtml", _settings.ToModel());
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            await _settingService.SaveSettingAsync(MickeySalePromoSettings.FromModel(model));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
