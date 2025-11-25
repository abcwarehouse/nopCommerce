using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.MickeySalePromo.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Topics;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Infrastructure;
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
        private readonly ITopicService _topicService;

        public MickeySalePromoController(
            MickeySalePromoSettings settings,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISettingService settingService,
            INopFileProvider fileProvider,
            ITopicService topicService)
        {
            _settings = settings;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _settingService = settingService;
            _fileProvider = fileProvider;
            _topicService = topicService;
        }

        public async Task<IActionResult> Configure()
        {
            var model = _settings.ToModel();

            // Populate available topics (storeId = 0 to load all)
            var topics = await _topicService.GetAllTopicsAsync(storeId: 0, showHidden: true);
            var topicsList = new List<SelectListItem>
            {
                new SelectListItem { Text = "-- Show on All Topic Pages --", Value = "0" }
            };
            topicsList.AddRange(topics.Select(t => new SelectListItem
            {
                Text = t.SystemName,
                Value = t.Id.ToString()
            }));
            model.AvailableTopics = topicsList;

            // Populate available widget zones with common zones for product promotions
            model.AvailableWidgetZones = new List<SelectListItem>
            {
                new SelectListItem { Text = "Home Page - Top", Value = PublicWidgetZones.HomepageTop },
                new SelectListItem { Text = "Home Page - Bottom", Value = PublicWidgetZones.HomepageBottom },
                new SelectListItem { Text = "Home Page - Before Products", Value = PublicWidgetZones.HomepageBeforeProducts },
                new SelectListItem { Text = "Home Page - Before Categories", Value = PublicWidgetZones.HomepageBeforeCategories },
                new SelectListItem { Text = "Category Details - Top", Value = PublicWidgetZones.CategoryDetailsTop },
                new SelectListItem { Text = "Category Details - Bottom", Value = PublicWidgetZones.CategoryDetailsBottom },
                new SelectListItem { Text = "Category Details - Before Product List", Value = PublicWidgetZones.CategoryDetailsBeforeProductList },
                new SelectListItem { Text = "Product Details - Top", Value = PublicWidgetZones.ProductDetailsTop },
                new SelectListItem { Text = "Product Details - Bottom", Value = PublicWidgetZones.ProductDetailsBottom },
                new SelectListItem { Text = "Product Details - Overview Top", Value = PublicWidgetZones.ProductDetailsOverviewTop },
                new SelectListItem { Text = "Product Details - Overview Bottom", Value = PublicWidgetZones.ProductDetailsOverviewBottom },
                new SelectListItem { Text = "Header - After", Value = PublicWidgetZones.HeaderAfter },
                new SelectListItem { Text = "Header - Before", Value = PublicWidgetZones.HeaderBefore },
                new SelectListItem { Text = "Content - Before", Value = PublicWidgetZones.ContentBefore },
                new SelectListItem { Text = "Content - After", Value = PublicWidgetZones.ContentAfter },
                new SelectListItem { Text = "Footer", Value = PublicWidgetZones.Footer }
            };

            return View("~/Plugins/Widgets.MickeySalePromo/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                // Start with current settings to preserve existing values
                var settings = new MickeySalePromoSettings
                {
                    TopBannerDesktopUrl = _settings.TopBannerDesktopUrl,
                    TopBannerMobileUrl = _settings.TopBannerMobileUrl,
                    LeftBannerUrl = _settings.LeftBannerUrl,
                    RightBannerUrl = _settings.RightBannerUrl,
                    WidgetZone = model.WidgetZone,
                    ProductsJson = model.Products != null && model.Products.Count > 0
                        ? JsonConvert.SerializeObject(model.Products)
                        : string.Empty,
                    TopicId = model.TopicId
                };

                // Create the upload directory if it doesn't exist in wwwroot/images
                // GetAbsolutePath automatically prepends WebRootPath
                var uploadPath = _fileProvider.GetAbsolutePath("images", "mickey-sale-promo");

                if (!_fileProvider.DirectoryExists(uploadPath))
                {
                    _fileProvider.CreateDirectory(uploadPath);
                }

                // Process Top Banner Desktop upload (only update if new file uploaded)
                if (model.TopBannerDesktopUpload != null && model.TopBannerDesktopUpload.Length > 0)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var originalFileName = Path.GetFileNameWithoutExtension(model.TopBannerDesktopUpload.FileName);
                    var extension = Path.GetExtension(model.TopBannerDesktopUpload.FileName);
                    var fileName = $"{originalFileName}-{timestamp}{extension}";
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
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var originalFileName = Path.GetFileNameWithoutExtension(model.TopBannerMobileUpload.FileName);
                    var extension = Path.GetExtension(model.TopBannerMobileUpload.FileName);
                    var fileName = $"{originalFileName}-{timestamp}{extension}";
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
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var originalFileName = Path.GetFileNameWithoutExtension(model.LeftBannerUpload.FileName);
                    var extension = Path.GetExtension(model.LeftBannerUpload.FileName);
                    var fileName = $"{originalFileName}-{timestamp}{extension}";
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
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var originalFileName = Path.GetFileNameWithoutExtension(model.RightBannerUpload.FileName);
                    var extension = Path.GetExtension(model.RightBannerUpload.FileName);
                    var fileName = $"{originalFileName}-{timestamp}{extension}";
                    var filePath = _fileProvider.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.RightBannerUpload.CopyToAsync(fileStream);
                    }

                    settings.RightBannerUrl = $"/images/mickey-sale-promo/{fileName}";
                }

                await _settingService.SaveSettingAsync(settings);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification($"Error saving configuration: {ex.Message}");
                return RedirectToAction("Configure");
            }

            // Redirect to force a fresh load of settings from the database
            return RedirectToAction("Configure");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBanner(string bannerType)
        {
            try
            {
                var settings = new MickeySalePromoSettings
                {
                    TopBannerDesktopUrl = _settings.TopBannerDesktopUrl,
                    TopBannerMobileUrl = _settings.TopBannerMobileUrl,
                    LeftBannerUrl = _settings.LeftBannerUrl,
                    RightBannerUrl = _settings.RightBannerUrl,
                    WidgetZone = _settings.WidgetZone,
                    ProductsJson = _settings.ProductsJson
                };

                // Clear the specified banner URL
                switch (bannerType)
                {
                    case "TopBannerDesktop":
                        settings.TopBannerDesktopUrl = string.Empty;
                        break;
                    case "TopBannerMobile":
                        settings.TopBannerMobileUrl = string.Empty;
                        break;
                    case "LeftBanner":
                        settings.LeftBannerUrl = string.Empty;
                        break;
                    case "RightBanner":
                        settings.RightBannerUrl = string.Empty;
                        break;
                }

                await _settingService.SaveSettingAsync(settings);
                _notificationService.SuccessNotification("Banner image removed successfully.");
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification($"Error removing banner: {ex.Message}");
            }

            return RedirectToAction("Configure");
        }
    }
}
