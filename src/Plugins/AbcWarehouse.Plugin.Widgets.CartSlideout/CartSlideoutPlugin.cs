using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Widgets.CartSlideout.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.AbcCore.Delivery;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Tasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Services.Localization;
using System;
using System.Net.Http;
using System.ComponentModel;
using Nop.Services.Orders;
using Nop.Core;
using Nop.Core.Domain.Orders;

namespace AbcWarehouse.Plugin.Widgets.CartSlideout
{
    public class CartSlideoutPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly string _taskType = $"{typeof(UpdateDeliveryOptionsTask).FullName}, {typeof(CartSlideoutPlugin).Namespace}";

        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryService _abcCategoryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        public CartSlideoutPlugin(
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            ICategoryService categoryService,
            ICategoryService abcCategoryService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _productAttributeService = productAttributeService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _categoryService = categoryService;
            _categoryService = abcCategoryService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        public bool HideInWidgetList => false;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "CartSlideout";
        }

        public System.Threading.Tasks.Task<IList<string>> GetWidgetZonesAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.BodyStartHtmlTagAfter });
        }

        public override async System.Threading.Tasks.Task InstallAsync()
        {
            // Need to have show cart after add to cart turned off:
            await _settingService.SetSettingAsync<bool>(
        "shoppingcartsettings.displaycartafteraddingproduct",
        false);

    await RemoveTaskAsync();
    await AddTaskAsync();

    await RemoveProductAttributesAsync();
    await AddProductAttributesAsync();

    var customer = await _workContext.GetCurrentCustomerAsync();
    // Get all cart items
    var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

    // Check if any product belongs to "Appliances"
    bool hasApplianceCategory = false;

    foreach (var item in cartItems)
    {
        // Get product categories
        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId);

        foreach (var productCategory in productCategories)
        {
            var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
            
            // Check if this category or its parent is "Appliances"
            if (category != null)
            {
                if (category.Name.Equals("Appliances", StringComparison.OrdinalIgnoreCase) ||
                    (category.ParentCategoryId > 0 && 
                    (await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId))?.Name.Equals("Appliances", StringComparison.OrdinalIgnoreCase) == true))
                {
                    hasApplianceCategory = true;
                    break;
                }
            }
        }

        if (hasApplianceCategory)
            break; // No need to check further if already found
    }

    // Update the localization message based on category presence
    if (hasApplianceCategory)
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "AbcWarehouse.Plugin.Widgets.CartSlideout.ApplianceDeliveryNotAvailable",
            "Home delivery for the zip code entered is not available through ABC Warehouse " +
            "because it is outside of our local service area in Michigan, Ohio and Indiana.  " +
            "Please see our sister company, us-appliance.com for nation-wide delivery options for " +
            "your new appliance(s). " +
            "<a href='https://www.us-appliance.com/' target='_blank' rel='noopener noreferrer'>Shop Us Appliance</a> for more details."
        );
    }
    else
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "AbcWarehouse.Plugin.Widgets.CartSlideout.DeliveryNotAvailable",
            "Home delivery for the zip code entered is not available at this time. ABC Warehouse currently " +
            "provides home delivery on major appliances and TVs within our Home Delivery Areas throughout " +
            "Michigan, and surrounding areas of our store locations in Ohio and Indiana."
        );
    }

    await _localizationService.AddOrUpdateLocaleResourceAsync(
        "AbcWarehouse.Plugin.Widgets.CartSlideout.MattressMessaging",
        "FREE Delivery, Set-Up and Removal on any mattress purchase set $697 or more."
    );

    await base.InstallAsync();
        }

        public override async System.Threading.Tasks.Task UninstallAsync()
        {
            await RemoveTaskAsync();
            await RemoveProductAttributesAsync();

            await base.UninstallAsync();
        }

        private async System.Threading.Tasks.Task AddTaskAsync()
        {
            ScheduleTask task = new ScheduleTask
            {
                Name = $"Update Delivery Options",
                Seconds = 14400,
                Type = _taskType,
                Enabled = true,
                StopOnError = false,
            };

            await _scheduleTaskService.InsertTaskAsync(task);
        }

        private async System.Threading.Tasks.Task RemoveTaskAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(_taskType);
            if (task != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(task);
            }
        }

        private async System.Threading.Tasks.Task AddProductAttributesAsync()
        {
            var pas = new ProductAttribute[]
            {
                new ProductAttribute() { Name = AbcDeliveryConsts.DeliveryPickupOptionsProductAttributeName },
                new ProductAttribute() { Name = AbcDeliveryConsts.HaulAwayDeliveryProductAttributeName },
                new ProductAttribute() { Name = AbcDeliveryConsts.HaulAwayDeliveryInstallProductAttributeName },
                new ProductAttribute() { Name = AbcDeliveryConsts.DeliveryAccessoriesProductAttributeName },
                new ProductAttribute() { Name = AbcDeliveryConsts.DeliveryInstallAccessoriesProductAttributeName },
                new ProductAttribute() { Name = AbcDeliveryConsts.PickupAccessoriesProductAttributeName }
            };

            foreach (var pa in pas)
            {
                await _productAttributeService.InsertProductAttributeAsync(pa);
            }
        }

        private async System.Threading.Tasks.Task RemoveProductAttributesAsync()
        {
            var attributes = (await _productAttributeService.GetAllProductAttributesAsync()).Where(pa =>
                pa.Name == AbcDeliveryConsts.DeliveryPickupOptionsProductAttributeName ||
                pa.Name == AbcDeliveryConsts.HaulAwayDeliveryProductAttributeName ||
                pa.Name == AbcDeliveryConsts.HaulAwayDeliveryInstallProductAttributeName ||
                pa.Name == AbcDeliveryConsts.DeliveryAccessoriesProductAttributeName ||
                pa.Name == AbcDeliveryConsts.DeliveryInstallAccessoriesProductAttributeName ||
                pa.Name == AbcDeliveryConsts.PickupAccessoriesProductAttributeName);

            foreach (var attribute in attributes)
            {
                await _productAttributeService.DeleteProductAttributeAsync(attribute);
            }
        }

        /*public async Task<Category> GetParentCategory(string zip, string productId)
        {
            string url = $"/AddToCart/GetDeliveryOptions?zip={zip}&productId={productId}";
            HttpResponseMessage response = null;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throw if not successful

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        } */
    }
}
