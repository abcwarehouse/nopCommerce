using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Seo;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Common;
using Nop.Services.Logging;
using Nop.Services.ExportImport;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport.Help;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.AbcCore.Nop
{
    public class AbcExportManager : ExportManager, IAbcExportManager
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public AbcExportManager(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            SecuritySettings securitySettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            IAddressService addressService,
            IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter,
            ICategoryService categoryService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IForumService forumService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IShipmentService shipmentService,
            ISpecificationAttributeService specificationAttributeService,
            IStateProvinceService stateProvinceService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ITaxCategoryService taxCategoryService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            ProductEditorSettings productEditorSettings
            )
        {}

        public async Task<byte[]> ExportPageNotFoundRecordsToXlsxAsync(IList<Log> logs)
        {
            var properties = new[]
            {
                new PropertyByName<Log>("Slug", l => l.PageUrl),
                new PropertyByName<Log>("Referrer", l => l.ReferrerUrl),
                new PropertyByName<Log>("Customer", async l => (await _customerService.GetCustomerByIdAsync(l.CustomerId.Value)).Email),
                new PropertyByName<Log>("IpAddress", l => l.IpAddress),
                new PropertyByName<Log>("Date", async l => (await _dateTimeHelper.ConvertToUserTimeAsync(l.CreatedOnUtc, DateTimeKind.Utc)).ToString("D")),
            };

            return await new PropertyManager<Log>(properties, _catalogSettings).ExportToXlsxAsync(logs);
        }
    }
}