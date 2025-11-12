using System;
using System.Linq;
using System.Threading.Tasks;

using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;

using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Tax.AbcTax.Services
{
    public class CustomOrderProcessingService : OrderProcessingService, IOrderProcessingService
    {
        // custom
        private readonly IWarrantyTaxService _warrantyTaxService;

        public CustomOrderProcessingService(
            CurrencySettings currencySettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ITaxService taxService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            // custom
            IWarrantyTaxService warrantyTaxService)
            : base(currencySettings,
                  addressService,
                  affiliateService,
                  checkoutAttributeFormatter,
                  countryService,
                  currencyService,
                  customerActivityService,
                  customerService,
                  customNumberFormatter,
                  discountService,
                  encryptionService,
                  eventPublisher,
                  genericAttributeService,
                  giftCardService,
                  languageService,
                  localizationService,
                  logger,
                  orderService,
                  orderTotalCalculationService,
                  paymentPluginManager,
                  paymentService,
                  pdfService,
                  priceCalculationService,
                  priceFormatter,
                  productAttributeFormatter,
                  productAttributeParser,
                  productService,
                  returnRequestService,
                  rewardPointService,
                  shipmentService,
                  shippingService,
                  shoppingCartService,
                  stateProvinceService,
                  staticCacheManager,
                  storeMappingService,
                  storeService,
                  taxService,
                  vendorService,
                  webHelper,
                  workContext,
                  workflowMessageService,
                  localizationSettings,
                  orderSettings,
                  paymentSettings,
                  rewardPointsSettings,
                  shippingSettings,
                  taxSettings)
        {
            // custom
            _warrantyTaxService = warrantyTaxService;
        }

        protected async override Task MoveShoppingCartItemsToOrderItemsAsync(
            PlaceOrderContainer details,
            Order order
        )
        {
            foreach (var sc in details.Cart)
            {
                var product = await _productService.GetProductByIdAsync(sc.ProductId);

                //prices
                var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
                var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
                // var scUnitPriceInclTax =
                //     await _taxService.GetProductPriceAsync(product, scUnitPrice, true, details.Customer);
                var scUnitPriceExclTax =
                    await _taxService.GetProductPriceAsync(product, scUnitPrice, false, details.Customer);
                // var scSubTotalInclTax =
                //     await _taxService.GetProductPriceAsync(product, scSubTotal, true, details.Customer);
                var scSubTotalExclTax =
                    await _taxService.GetProductPriceAsync(product, scSubTotal, false, details.Customer);

                // custom - getting warranty tax
                var (_, scSubTotalInclTax, scUnitPriceInclTax, _, _) = await _warrantyTaxService.CalculateWarrantyTaxAsync(sc, details.Customer, scSubTotalExclTax.price, scUnitPriceExclTax.price);

                var discountAmountInclTax =
                    await _taxService.GetProductPriceAsync(product, discountAmount, true, details.Customer);
                var discountAmountExclTax =
                    await _taxService.GetProductPriceAsync(product, discountAmount, false, details.Customer);
                foreach (var disc in scDiscounts)
                    if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                        details.AppliedDiscounts.Add(disc);

                

                //attributes
                var attributeDescription =
                    await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, details.Customer);

                var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);

                //save order item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = scUnitPriceInclTax,
                    UnitPriceExclTax = scUnitPriceExclTax.price,
                    PriceInclTax = scSubTotalInclTax,
                    PriceExclTax = scSubTotalExclTax.price,
                    OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DiscountAmountInclTax = discountAmountInclTax.price,
                    DiscountAmountExclTax = discountAmountExclTax.price,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc
                };

                await _orderService.InsertOrderItemAsync(orderItem);

                //gift cards
                await AddGiftCardsAsync(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax.price);

                //inventory
                await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
            }

            //clear shopping cart
            details.Cart.ToList().ForEach(async sci => await _shoppingCartService.DeleteShoppingCartItemAsync(sci, false));
        }
    }
}
