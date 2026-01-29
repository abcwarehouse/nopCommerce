using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.AbcCore.Components;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Plugin.Misc.AbcCore.Tasks;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using SevenSpikes.Nop.Plugins.HtmlWidgets.Domain;

namespace Nop.Plugin.Misc.AbcCore
{
    public class CorePlugin : BasePlugin, IMiscPlugin, IWidgetPlugin,
        IConsumer<EntityDeletedEvent<ProductPicture>>,
        IConsumer<EntityUpdatedEvent<HtmlWidget>>,
        IConsumer<AdminMenuCreatedEvent>
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INopDataProvider _nopDataProvider;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IPictureService _pictureService;
        private readonly IProductAbcDescriptionService _productAbcDescriptionService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStaticCacheManager _cacheManager;

        public CorePlugin(
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILogger logger,
            INopDataProvider nopDataProvider,
            INopFileProvider nopFileProvider,
            IPictureService pictureService,
            IProductAbcDescriptionService productAbcDescriptionService,
            IScheduleTaskService scheduleTaskService,
            IWebHostEnvironment webHostEnvironment,
            IStaticCacheManager cacheManager
        )
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _logger = logger;
            _nopDataProvider = nopDataProvider;
            _nopFileProvider = nopFileProvider;
            _pictureService = pictureService;
            _productAbcDescriptionService = productAbcDescriptionService;
            _scheduleTaskService = scheduleTaskService;
            _webHostEnvironment = webHostEnvironment;
            _cacheManager = cacheManager;
        }

        public async System.Threading.Tasks.Task HandleEventAsync(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            // Is this an ABC product with an ABC Item Number?
            var pad = await _productAbcDescriptionService.GetProductAbcDescriptionByProductIdAsync(eventMessage.Entity.ProductId);
            if (pad == null) { return; }

            // Is there a picture in product_images?
            var abcProductImagePath = _nopFileProvider.GetFiles("wwwroot/product_images", $"{pad.AbcItemNumber}_large.*").FirstOrDefault();
            if (abcProductImagePath == null) { return; }

            // Are they the same picture? If so delete.
            var nopPictureBinary = await _pictureService.GetPictureBinaryByPictureIdAsync(eventMessage.Entity.PictureId);
            var fileSystemBinary = await _nopFileProvider.ReadAllBytesAsync(abcProductImagePath);
            if (nopPictureBinary.BinaryData.SequenceEqual(fileSystemBinary))
            {
                _nopFileProvider.DeleteFile(abcProductImagePath);
                await _logger.InformationAsync($"Deleted image `{abcProductImagePath}` (image deleted in NOP)");
            }
        }

        // Cleans additional cache for HTML Widgets after changing content to reflect changes immediately
        public async System.Threading.Tasks.Task HandleEventAsync(EntityUpdatedEvent<HtmlWidget> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync("Nop.conditionstatement.all.");
        }

        public System.Threading.Tasks.Task<IList<string>> GetWidgetZonesAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IList<string>>(
                new List<string>
                {
                    AdminWidgetZones.ProductDetailsBlock,
                    AdminWidgetZones.HeaderBefore,
                    PublicWidgetZones.Footer,
                    AdminWidgetZones.CategoryDetailsBlock
                }
            );
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            ArgumentNullException.ThrowIfNull(widgetZone);

            return typeof(AbcCoreViewComponent);
        }

        public bool HideInWidgetList => false;

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AbcCore/Configure";
        }

        private async System.Threading.Tasks.Task DeleteStoredProcs()
        {
            await _nopDataProvider.ExecuteNonQueryAsync("DROP PROCEDURE IF EXISTS dbo.UpdateAbcPromos");
        }

        private async System.Threading.Tasks.Task InstallStoredProcs()
        {
            await DeleteStoredProcs();
            string updateAbcPromosStoredProcScript = File.ReadAllText(
                            $"{_webHostEnvironment.ContentRootPath}/Plugins/Misc.AbcCore/UpdateAbcPromos.StoredProc.sql"
                        );
            await _nopDataProvider.ExecuteNonQueryAsync(updateAbcPromosStoredProcScript);
        }

        // https://docs.nopcommerce.com/en/developer/plugins/menu-item.html
        public Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            eventMessage.RootMenuItem.InsertBefore("All plugins and themes",
                new AdminMenuItem
                {
                    SystemName = "ABCWarehouse",
                    Title = "ABC Warehouse",
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "ABCWarehouse.Promos",
                            Title = "Promos",
                            Url = eventMessage.GetMenuItemUrl("AbcPromo", "List"),
                            IconClass = "far fa-dot-circle",
                            Visible = true
                        },
                        new()
                        {
                            SystemName = "ABCWarehouse.MissingImageProducts",
                            Title = "Missing Image Products",
                            Url = eventMessage.GetMenuItemUrl("MissingImageProducts", "List"),
                            IconClass = "far fa-dot-circle",
                            Visible = true
                        },
                        new()
                        {
                            SystemName = "ABCWarehouse.NewProducts",
                            Title = "New Products",
                            Url = eventMessage.GetMenuItemUrl("NewProduct", "List"),
                            IconClass = "far fa-dot-circle",
                            Visible = true
                        },
                        new()
                        {
                            SystemName = "ABCWarehouse.PageNotFound",
                            Title = "Page Not Found List",
                            Url = eventMessage.GetMenuItemUrl("PageNotFound", "List"),
                            IconClass = "far fa-dot-circle",
                            Visible = true
                        },
                        new()
                        {
                            SystemName = "ABCWarehouse.PageNotFoundFreq",
                            Title = "Page Not Found Frequency",
                            Url = eventMessage.GetMenuItemUrl("PageNotFound", "Frequency"),
                            IconClass = "far fa-dot-circle",
                            Visible = true
                        }
                    }
                }
            );

            return Task.CompletedTask;
        }
    }
}
