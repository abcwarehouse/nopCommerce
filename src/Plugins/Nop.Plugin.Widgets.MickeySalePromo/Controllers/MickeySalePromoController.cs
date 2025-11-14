using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
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
        private readonly INopFileProvider _fileProvider;

        public MickeySalePromoController(
            MickeySalePromoSettings settings,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISettingService settingService,
            INopFileProvider fileProvider)
        {
            _settings = settings;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _settingService = settingService;
            _fileProvider = fileProvider;
        }

        public IActionResult Configure()
        {
            return View("~/Plugins/Widgets.MickeySalePromo/Views/Configure.cshtml", _settings.ToModel());
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            // Debug: Log what we're receiving
            var productCount = model.Products?.Count ?? 0;
            System.Diagnostics.Debug.WriteLine($"Products received: {productCount}");

            if (model.Products != null)
            {
                for (int i = 0; i < model.Products.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Product {i}: ItemNumber={model.Products[i].ItemNumber}, Sku={model.Products[i].Sku}");
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"ModelState Errors: {string.Join(", ", errors)}");
                return Configure();
            }

            // Start with current settings to preserve existing values
            var settings = new MickeySalePromoSettings
            {
                TopBannerDesktopUrl = _settings.TopBannerDesktopUrl,
                TopBannerMobileUrl = _settings.TopBannerMobileUrl,
                LeftBannerUrl = _settings.LeftBannerUrl,
                RightBannerUrl = _settings.RightBannerUrl,
                ProductsJson = model.Products != null && model.Products.Count > 0
                    ? JsonConvert.SerializeObject(model.Products)
                    : string.Empty
            };

            System.Diagnostics.Debug.WriteLine($"ProductsJson being saved: {settings.ProductsJson}");

            // Create the upload directory if it doesn't exist
            var uploadPath = _fileProvider.MapPath("~/images/mickey-sale-promo");
            _fileProvider.CreateDirectory(uploadPath);

            // Process Top Banner Desktop upload (only update if new file uploaded)
            if (model.TopBannerDesktopUpload != null && model.TopBannerDesktopUpload.Length > 0)
            {
                var fileName = $"top-banner-desktop{Path.GetExtension(model.TopBannerDesktopUpload.FileName)}";
                var filePath = _fileProvider.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.TopBannerDesktopUpload.CopyToAsync(fileStream);
                }

                settings.TopBannerDesktopUrl = $"/images/mickey-sale-promo/{fileName}";
            }

            // Process Top Banner Mobile upload (only update if new file uploaded)
            if (model.TopBannerMobileUpload != null && model.TopBannerMobileUpload.Length > 0)
            {
                var fileName = $"top-banner-mobile{Path.GetExtension(model.TopBannerMobileUpload.FileName)}";
                var filePath = _fileProvider.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.TopBannerMobileUpload.CopyToAsync(fileStream);
                }

                settings.TopBannerMobileUrl = $"/images/mickey-sale-promo/{fileName}";
            }

            // Process Left Banner upload (only update if new file uploaded)
            if (model.LeftBannerUpload != null && model.LeftBannerUpload.Length > 0)
            {
                var fileName = $"left-banner{Path.GetExtension(model.LeftBannerUpload.FileName)}";
                var filePath = _fileProvider.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LeftBannerUpload.CopyToAsync(fileStream);
                }

                settings.LeftBannerUrl = $"/images/mickey-sale-promo/{fileName}";
            }

            // Process Right Banner upload (only update if new file uploaded)
            if (model.RightBannerUpload != null && model.RightBannerUpload.Length > 0)
            {
                var fileName = $"right-banner{Path.GetExtension(model.RightBannerUpload.FileName)}";
                var filePath = _fileProvider.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.RightBannerUpload.CopyToAsync(fileStream);
                }

                settings.RightBannerUrl = $"/images/mickey-sale-promo/{fileName}";
            }

            await _settingService.SaveSettingAsync(settings);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
