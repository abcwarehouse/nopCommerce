using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Nop.Plugin.Misc.AbcCore.Mattresses;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Plugin.Misc.AbcCore.Delivery;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Models.Media;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Plugin.Misc.AbcCore.Factories
{
    public partial class AbcProductModelFactory : ProductModelFactory, IAbcProductModelFactory
    {
        private readonly IAbcMattressListingPriceService _abcMattressListingPriceService;
        private readonly IProductAbcDescriptionService _productAbcDescriptionService;

        public AbcProductModelFactory(
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IJsonLdModelFactory jsonLdModelFactory,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreService storeService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IVideoService videoService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            ShippingSettings shippingSettings,
            VendorSettings vendorSettings,
            // ABC: custom services
            IAbcMattressListingPriceService abcMattressListingPriceService,
            IProductAbcDescriptionService productAbcDescriptionService)
            : base(captchaSettings,
                  catalogSettings,
                  customerSettings,
                  categoryService,
                  currencyService,
                  customerService,
                  dateRangeService,
                  dateTimeHelper,
                  downloadService,
                  genericAttributeService,
                  jsonLdModelFactory,
                  localizationService,
                  manufacturerService,
                  permissionService,
                  pictureService,
                  priceCalculationService,
                  priceFormatter,
                  productAttributeParser,
                  productAttributeService,
                  productService,
                  productTagService,
                  productTemplateService,
                  reviewTypeService,
                  shoppingCartService,
                  specificationAttributeService,
                  staticCacheManager,
                  storeContext,
                  storeService,
                  shoppingCartModelFactory,
                  taxService,
                  urlRecordService,
                  vendorService,
                  videoService,
                  webHelper,
                  workContext,
                  mediaSettings,
                  orderSettings,
                  seoSettings,
                  shippingSettings,
                  vendorSettings
            )
        {
            _abcMattressListingPriceService = abcMattressListingPriceService;
            _productAbcDescriptionService = productAbcDescriptionService;
        }

        protected override async Task<ProductPriceModel>
            PrepareProductPriceModelAsync(Product product, bool addPriceRangeFrom = false, bool forceRedirectionAfterAddingToCart = false)
        {
            var model = await base.PrepareProductPriceModelAsync(product, addPriceRangeFrom, forceRedirectionAfterAddingToCart);
            var prices = await AdjustMattressPriceAsync(product.Id);
            model.Price = prices.price ?? model.Price;
            model.OldPrice = prices.oldPrice ?? model.OldPrice;

            return model;
        }

        // Excludes product attributes that shouldn't appear by default
        protected override async Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModelsAsync(
            Product product,
            ShoppingCartItem updatecartitem
        ) {
            var models = await base.PrepareProductAttributeModelsAsync(product, updatecartitem);

            return models.Where(m => !new string[]{
                "Home Delivery",
                "Warranty",
                AbcDeliveryConsts.DeliveryPickupOptionsProductAttributeName,
                "Pickup",
                "Haul Away (Delivery)",
                "Haul Away (Delivery/Install)",
                "FedEx",
                AbcDeliveryConsts.DeliveryAccessoriesProductAttributeName,
                AbcDeliveryConsts.DeliveryInstallAccessoriesProductAttributeName,
                AbcDeliveryConsts.PickupAccessoriesProductAttributeName
            }.Contains(m.Name)).ToList();
        }

        // Allows for returning specific attributes
        public async Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModelsAsync(
            Product product,
            ShoppingCartItem updatecartitem,
            string[] attributesToInclude
        ) {
            var models = await base.PrepareProductAttributeModelsAsync(product, updatecartitem);
            var filteredModels = models.Where(m => attributesToInclude.Contains(m.Name)).ToList();

            // Warranty needs to be pre-selected
            if (updatecartitem != null)
            {
                var warrantyModel = filteredModels.FirstOrDefault(m => m.Name == "Warranty");
                if (warrantyModel != null)
                {
                    var selectedValue = (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml, warrantyModel.Id)).FirstOrDefault();
                    if (selectedValue != null)
                    {
                        selectedValue.IsPreSelected = true;
                    }
                }
            }
            

            return filteredModels;
        }

        protected override async Task<IList<PictureModel>> PrepareProductOverviewPicturesModelAsync(Product product, int? productThumbPictureSize = null)
        {
            ArgumentNullException.ThrowIfNull(product);

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductOverviewPicturesModelKey,
                product, pictureSize, true, _catalogSettings.DisplayAllPicturesOnCatalogPages, await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var cachedPictures = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                async Task<PictureModel> preparePictureModelAsync(Picture picture)
                {
                    //we have to keep the url generation order "full size -> preview" because picture can be updated twice
                    //this section of code requires detailed analysis in the future
                    (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (var imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                    return new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        //"title" attribute
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                            ? picture.TitleAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                                productName),
                        //"alt" attribute
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                            ? picture.AltAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                                productName)
                    };
                }

                //all pictures
                // ABC: get 2 pictures to account for hover image
                var pictures = (await _pictureService
                        .GetPicturesByProductIdAsync(product.Id, _catalogSettings.DisplayAllPicturesOnCatalogPages ? 0 : 2))
                    .DefaultIfEmpty(null);
                var pictureModels = await pictures
                    .SelectAwait(async picture => await preparePictureModelAsync(picture))
                    .ToListAsync();
                return pictureModels;
            });

            return cachedPictures;
        }

        protected override async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels, IList<VideoModel> allVideoModels)> PrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            ArgumentNullException.ThrowIfNull(product);

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                (var fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);
                (var imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                        defaultPicture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                    //"alt" attribute
                    AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                        defaultPicture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName)
                };

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count; i++)
                {
                    var picture = pictures[i];

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (var thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        Id = picture.Id,
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;

            //all videos
            var allvideoModels = new List<VideoModel>();
            var videos = await _videoService.GetVideosByProductIdAsync(product.Id);
            foreach (var video in videos)
            {
                var videoModel = new VideoModel
                {
                    VideoUrl = video.VideoUrl,
                    // ABC: custom to allow thumbnails for videos
                    ThumbnailUrl = video.ThumbnailUrl,
                    Allow = _mediaSettings.VideoIframeAllow,
                    Width = _mediaSettings.VideoIframeWidth,
                    Height = _mediaSettings.VideoIframeHeight
                };

                allvideoModels.Add(videoModel);
            }
            return (cachedPictures.DefaultPictureModel, allPictureModels, allvideoModels);
        }

        private async Task<(string price, string oldPrice)> AdjustMattressPriceAsync(int productId)
        {
            var mattressPrice = await _abcMattressListingPriceService.GetListingPriceForMattressProductAsync(
                productId
            );
            if (mattressPrice.HasValue)
            {
                var price = (await _priceFormatter.FormatPriceAsync(mattressPrice.Value.Price)).Replace(".00", "");
                var oldPrice = (await _priceFormatter.FormatPriceAsync(mattressPrice.Value.OldPrice)).Replace(".00", "");
                return (price, oldPrice);
            }

            return (null, null);
        }
    }
}
