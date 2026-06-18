using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AbcFrontend.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority
        {
            get
            {
                return int.MaxValue;
            }
        }

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("CustomCheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "AbcCheckout", action = "ShippingMethod" });

            endpointRouteBuilder.MapControllerRoute("CustomCheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "AbcCheckout", action = "PaymentMethod" });

            endpointRouteBuilder.MapControllerRoute("CustomCheckoutPaymentInfo",
                            "checkout/paymentinfo",
                            new { controller = "AbcCheckout", action = "PaymentInfo" });

            endpointRouteBuilder.MapControllerRoute("CustomCheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "AbcCheckout", action = "Confirm" });

            endpointRouteBuilder.MapControllerRoute("WarrantySelection",
                            "checkout/warranty",
                            new { controller = "AbcCheckout", action = "WarrantySelection" });

            endpointRouteBuilder.MapControllerRoute("CustomCheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "AbcCheckout", action = "OnePageCheckout" });

            endpointRouteBuilder.MapControllerRoute("ShoppingCartRemoveItem",
                            "ShoppingCart/RemoveItem",
                            new { controller = "AbcShoppingCart", action = "RemoveItem" });

            endpointRouteBuilder.MapControllerRoute("AddProductToCart-Pickup",
                            "addproducttocart/pickup/{productId}/{shoppingCartTypeId}",
                            new { controller = "AbcShoppingCart", action = "AddProductToCart_Pickup" },
                            new { productId = @"\d+", shoppingCartTypeId = @"\d+" });

            endpointRouteBuilder.MapControllerRoute("CustomAddProductToCart-Catalog",
                            "addproducttocart/catalog/{productId}/{shoppingCartTypeId}/{quantity}",
                            new { controller = "AbcShoppingCart", action = "AddProductToCart_Catalog" },
                            new { productId = @"\d+", shoppingCartTypeId = @"\d+", quantity = @"\d+" });

            endpointRouteBuilder.MapControllerRoute("CustomAddProductToCart-Details",
                            "addproducttocart/details/{productId}/{shoppingCartTypeId}",
                            new { controller = "AbcShoppingCart", action = "AddProductToCart_Details" },
                            new { productId = @"\d+", shoppingCartTypeId = @"\d+" });
        }
    }
}
