using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.AbcCore.Factories
{
    public partial class AbcCategoryModelFactory : CategoryModelFactory, ICategoryModelFactory
    {
        public AbcCategoryModelFactory(
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService)
            : base(catalogSettings, currencySettings,
            currencyService, aclSupportedModelFactory,
            baseAdminModelFactory, categoryService, discountService,
            discountSupportedModelFactory, localizationService, localizedModelFactory,
            productService, storeMappingSupportedModelFactory,
            urlRecordService
            )
        {
        }

        public override async Task<CategoryModel> PrepareCategoryModelAsync(CategoryModel model, Category category, bool excludeProperties = false)
        {
            var newModel = await base.PrepareCategoryModelAsync(model, category, excludeProperties);

            // check if we're in Hawthorne and if we need to change out images

            return newModel;
        }
    }
}