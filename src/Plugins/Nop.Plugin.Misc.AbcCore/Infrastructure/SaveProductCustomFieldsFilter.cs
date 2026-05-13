using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Catalog;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.AbcCore.Infrastructure
{
    /// <summary>
    /// Action filter to save custom product fields (like PLPDescription) from the admin product edit form
    /// </summary>
    public class SaveProductCustomFieldsFilter : IAsyncActionFilter
    {
        private readonly IProductService _productService;
        private readonly IGenericAttributeService _genericAttributeService;

        public SaveProductCustomFieldsFilter(
            IProductService productService,
            IGenericAttributeService genericAttributeService)
        {
            _productService = productService;
            _genericAttributeService = genericAttributeService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only process POST requests to Product/Edit
            if (context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase) &&
                context.RouteData.Values["controller"]?.ToString() == "Product" &&
                context.RouteData.Values["action"]?.ToString() == "Edit" &&
                context.RouteData.Values["area"]?.ToString() == "Admin")
            {
                var plpDescription = context.HttpContext.Request.Form["PLPDescription"].ToString();
                var abcShortDescription = context.HttpContext.Request.Form["AbcShortDescription"].ToString();
                var abcFullDescription = context.HttpContext.Request.Form["AbcFullDescription"].ToString();
                var productId = Convert.ToInt32(context.RouteData.Values["id"].ToString());

                var product = await _productService.GetProductByIdAsync(productId);

                await _genericAttributeService.SaveAttributeAsync(
                    product, "PLPDescription", plpDescription
                );
                await _genericAttributeService.SaveAttributeAsync(
                    product, "AbcShortDescription", abcShortDescription
                );
                await _genericAttributeService.SaveAttributeAsync(
                    product, "AbcFullDescription", abcFullDescription
                );
            }

            await next();
        }
    }
}
