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
                // Resolve product ID from route or form
                int productId = 0;
                if (context.RouteData.Values.TryGetValue("id", out var idValue))
                    int.TryParse(idValue?.ToString(), out productId);
                if (productId == 0 && context.HttpContext.Request.Form.TryGetValue("Id", out var formIdValue))
                    int.TryParse(formIdValue.ToString(), out productId);

                if (productId > 0)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        // Save PLPDescription
                        if (context.HttpContext.Request.Form.TryGetValue("PLPDescription", out var plpDescriptionValues))
                        {
                            await _genericAttributeService.SaveAttributeAsync(
                                product, "PLPDescription", plpDescriptionValues.ToString()
                            );
                        }
                    }
                }
            }

            await next();
        }
    }
}
