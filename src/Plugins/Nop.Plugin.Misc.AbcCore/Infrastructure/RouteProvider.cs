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

            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageList",
                            "Admin/MickeyLandingPage/List",
                            new { controller = "MickeyLandingPage", action = "List", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageCreate",
                            "Admin/MickeyLandingPage/Create",
                            new { controller = "MickeyLandingPage", action = "Create", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageEdit",
                            "Admin/MickeyLandingPage/Edit/{id}",
                            new { controller = "MickeyLandingPage", action = "Edit", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageDelete",
                            "Admin/MickeyLandingPage/Delete",
                            new { controller = "MickeyLandingPage", action = "Delete", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageProductList",
                            "Admin/MickeyLandingPage/ProductList",
                            new { controller = "MickeyLandingPage", action = "ProductList", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageAddProduct",
                            "Admin/MickeyLandingPage/AddProduct",
                            new { controller = "MickeyLandingPage", action = "AddProduct", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageRemoveProduct",
                            "Admin/MickeyLandingPage/RemoveProduct",
                            new { controller = "MickeyLandingPage", action = "RemoveProduct", area = "Admin" });
            endpointRouteBuilder.MapControllerRoute("MickeyLandingPageGetProductLandingPages",
                            "Admin/MickeyLandingPage/GetProductLandingPages",
                            new { controller = "MickeyLandingPage", action = "GetProductLandingPages", area = "Admin" });


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
