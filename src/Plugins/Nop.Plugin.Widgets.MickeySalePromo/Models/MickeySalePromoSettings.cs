using Nop.Core.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Widgets.MickeySalePromo.Models
{
    public class MickeySalePromoSettings : ISettings
    {
        public string TopBannerDesktopUrl { get; set; }
        public string TopBannerMobileUrl { get; set; }
        public string LeftBannerUrl { get; set; }
        public string RightBannerUrl { get; set; }

        // Widget zone where all products will be displayed
        public string WidgetZone { get; set; }

        // Store products as JSON string
        public string ProductsJson { get; set; }

        // Selected topic page ID (0 = show on all topic pages)
        public int TopicId { get; set; }

        internal ConfigurationModel ToModel()
        {
            var products = string.IsNullOrEmpty(ProductsJson)
                ? new List<SaleProductModel>()
                : JsonConvert.DeserializeObject<List<SaleProductModel>>(ProductsJson);

            return new ConfigurationModel
            {
                TopBannerDesktopUrl = TopBannerDesktopUrl,
                TopBannerMobileUrl = TopBannerMobileUrl,
                LeftBannerUrl = LeftBannerUrl,
                RightBannerUrl = RightBannerUrl,
                WidgetZone = WidgetZone,
                Products = products,
                TopicId = TopicId
            };
        }

        internal static MickeySalePromoSettings FromModel(ConfigurationModel model)
        {
            var productsJson = model.Products != null && model.Products.Count > 0
                ? JsonConvert.SerializeObject(model.Products)
                : string.Empty;

            return new MickeySalePromoSettings
            {
                TopBannerDesktopUrl = model.TopBannerDesktopUrl,
                TopBannerMobileUrl = model.TopBannerMobileUrl,
                LeftBannerUrl = model.LeftBannerUrl,
                RightBannerUrl = model.RightBannerUrl,
                WidgetZone = model.WidgetZone,
                ProductsJson = productsJson,
                TopicId = model.TopicId
            };
        }

        public List<SaleProductModel> GetProducts()
        {
            return string.IsNullOrEmpty(ProductsJson)
                ? new List<SaleProductModel>()
                : JsonConvert.DeserializeObject<List<SaleProductModel>>(ProductsJson);
        }
    }
}
