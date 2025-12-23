using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AbcCore.Infrastructure
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
            // AbcCategory - get
            endpointRouteBuilder.MapControllerRoute("AbcCategoryEditGet",
                            "Admin/Category/Edit/{id}",
                            new { controller = "AbcCategory", action = "Edit", area = "Admin" });

            // AbcCategory - post
            endpointRouteBuilder.MapControllerRoute("AbcCategoryEditPost",
                            "Admin/Category/Edit/{id?}",
                            new { controller = "AbcCategory", action = "Edit", area = "Admin" });

            endpointRouteBuilder.MapControllerRoute("AbcPromoProductList",
                            "Admin/AbcPromo/Products/{abcPromoId}",
                            new { controller = "AbcPromo", action = "Products", area = "Admin" });


            // Add to Cart Slideout
            endpointRouteBuilder.MapControllerRoute("CartSlideout_GetDeliveryOptions",
                            "AddToCart/GetDeliveryOptions",
                            new { controller = "CartSlideout", action = "GetDeliveryOptions"});
            endpointRouteBuilder.MapControllerRoute("CartSlideout_GetEditCartItemInfo",
                            "AddToCart/GetEditCartItemInfo",
                            new { controller = "CartSlideout", action = "GetEditCartItemInfo"});

            endpointRouteBuilder.MapControllerRoute("GetProductAttributeValue",
                            "api/ProductAttributeValue/{productAttributeValueId}",
                            new { controller = "Api", action = "GetProductAttributeValue"});
            endpointRouteBuilder.MapControllerRoute("CartSlideout_GetAddCartItemInfo",
                            "AddToCart/GetAddCartItemInfo",
                            new { controller = "CartSlideout", action = "GetAddCartItemInfo"});
        }
    }
}
