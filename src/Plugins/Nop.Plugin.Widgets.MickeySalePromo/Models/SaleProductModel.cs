using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MickeySalePromo.Models
{
    public record SaleProductModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ItemNumber")]
        public string ItemNumber { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.BrandName")]
        public string BrandName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.Sku")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ProductUrl")]
        public string ProductUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ImageUrl")]
        public string ImageUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.Banner")]
        public string Banner { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.OldPrice")]
        public string OldPrice { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.YouSave")]
        public string YouSave { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ActualPrice")]
        public string ActualPrice { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.ShowDisclaimer")]
        public bool ShowDisclaimer { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.Product.WidgetZone")]
        public string WidgetZone { get; set; }
    }
}
