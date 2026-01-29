using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Core.Domain.Common;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Controllers
{
    public class AbcProductController : ProductController
    {
        public AbcProductController(AdminAreaSettings adminAreaSettings,
            IAclService aclService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICategoryService categoryService,
            ICopyProductService copyProductService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IExportManager exportManager,
            IGenericAttributeService genericAttributeService,
            IHttpClientFactory httpClientFactory,
            IImportManager importManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IVideoService videoService,
            IWebHelper webHelper,
            IWorkContext workContext,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings

        ) : base(adminAreaSettings,
            aclService,
            backInStockSubscriptionService,
            categoryService,
            copyProductService,
            currencyService,
            customerActivityService,
            customerService,
            discountService,
            downloadService,
            exportManager,
            genericAttributeService,
            httpClientFactory,
            importManager,
            languageService,
            localizationService,
            localizedEntityService,
            manufacturerService,
            fileProvider,
            notificationService,
            pdfService,
            permissionService,
            pictureService,
            productAttributeFormatter,
            productAttributeParser,
            productAttributeService,
            productModelFactory,
            productService,
            productTagService,
            settingService,
            shippingService,
            shoppingCartService,
            specificationAttributeService,
            storeContext,
            urlRecordService,
            videoService,
            webHelper,
            workContext,
            currencySettings,
            taxSettings,
            vendorSettings
        )
        { }

        public override async Task<IActionResult> Edit(int id)
        {
            return await base.Edit(id);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public override async Task<IActionResult> Edit(ProductModel model, bool continueEditing)
        {
            // Saves PLP description
            var plpDescription = Request.Form["PLPDescription"].ToString();
            var product = await _productService.GetProductByIdAsync(model.Id);
            await _genericAttributeService.SaveAttributeAsync<string>(
                product, "PLPDescription", plpDescription
            );

            return await base.Edit(model, continueEditing);
        }
    }
}
