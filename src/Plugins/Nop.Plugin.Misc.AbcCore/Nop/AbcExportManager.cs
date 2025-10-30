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
using Nop.Plugin.Misc.AbcCore.Domain;

namespace Nop.Plugin.Misc.AbcCore.Nop
{
    public class AbcExportManager : ExportManager, IAbcExportManager
    {
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
        ) : base(addressSettings,
            catalogSettings,
            securitySettings,
            customerSettings,
            dateTimeSettings,
            forumSettings,
            addressService,
            customerAttributeFormatter,
            categoryService,
            countryService,
            currencyService,
            customerActivityService,
            customerService,
            dateRangeService,
            dateTimeHelper,
            discountService,
            forumService,
            gdprService,
            genericAttributeService,
            languageService,
            localizationService,
            localizedEntityService,
            manufacturerService,
            measureService,
            newsLetterSubscriptionService,
            orderService,
            pictureService,
            priceFormatter,
            productAttributeService,
            productService,
            productTagService,
            productTemplateService,
            shipmentService,
            specificationAttributeService,
            stateProvinceService,
            storeMappingService,
            storeService,
            taxCategoryService,
            urlRecordService,
            vendorService,
            workContext,
            orderSettings,
            productEditorSettings
        )
        {}

        public async Task<byte[]> ExportPageNotFoundRecordsToXlsxAsync(IList<PageNotFoundRecord> pageNotFoundRecords)
        {
            var properties = new[]
            {
                new PropertyByName<PageNotFoundRecord>("Slug", l => l.Slug),
                new PropertyByName<PageNotFoundRecord>("Referrer", l => l.Referrer),
                new PropertyByName<PageNotFoundRecord>("Customer", async l => {
                    var cust = await _customerService.GetCustomerByIdAsync(l.CustomerId);
                    return cust?.Email ?? string.Empty;
                }),
                new PropertyByName<PageNotFoundRecord>("IpAddress", l => l.IpAddress),
                new PropertyByName<PageNotFoundRecord>("Date", async l => (await _dateTimeHelper.ConvertToUserTimeAsync(l.CreatedOnUtc, DateTimeKind.Utc)).ToString("D")),
            };

            return await new PropertyManager<PageNotFoundRecord>(properties, _catalogSettings).ExportToXlsxAsync(pageNotFoundRecords);
        }
    }
}