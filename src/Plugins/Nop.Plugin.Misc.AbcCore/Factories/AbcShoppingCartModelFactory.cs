using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.AbcCore.Factories
{
    public class AbcShoppingCartModelFactory : ShoppingCartModelFactory, IShoppingCartModelFactory
    {
        private readonly IPriceFormatter _priceFormatter;
        
        public AbcShoppingCartModelFactory(
            CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStoreService storeService,
            ITaxService taxService) :
            base(
                catalogSettings,
                baseAdminModelFactory,
                countryService,
                customerService,
                dateTimeHelper,
                localizationService,
                priceFormatter,
                productAttributeFormatter,
                productService,
                shoppingCartService,
                storeService,
                taxService
            )
        {
            _priceFormatter = priceFormatter;
        }
        
        protected override async Task<ShoppingCartModel.ShoppingCartItemModel> PrepareShoppingCartItemModelAsync(
            IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            var model = await base.PrepareShoppingCartItemModelAsync(cart, sci);

            // ABCTODO: consider adding the mattress/delivery option functionality
            // here as well

            if (model.Discount == null) { return model; }
            var discountValue = Decimal.Parse(
                model.Discount,
                NumberStyles.AllowCurrencySymbol |
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowThousands, new CultureInfo("en-US")) / model.Quantity;
            var unitPriceValue = Decimal.Parse(
                model.UnitPrice,
                NumberStyles.AllowCurrencySymbol |
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowThousands, new CultureInfo("en-US"));

            model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceValue + discountValue);

            return model;
        }
    }
}
