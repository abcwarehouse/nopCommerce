using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.MickeySalePromo.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            Products = new List<SaleProductModel>();
            AvailableWidgetZones = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerDesktopUrl")]
        public string TopBannerDesktopUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerMobileUrl")]
        public string TopBannerMobileUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.LeftBannerUrl")]
        public string LeftBannerUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.RightBannerUrl")]
        public string RightBannerUrl { get; set; }

        // File upload properties
        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerDesktopUpload")]
        public IFormFile TopBannerDesktopUpload { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopBannerMobileUpload")]
        public IFormFile TopBannerMobileUpload { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.LeftBannerUpload")]
        public IFormFile LeftBannerUpload { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.RightBannerUpload")]
        public IFormFile RightBannerUpload { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.WidgetZone")]
        public string WidgetZone { get; set; }

        public List<SaleProductModel> Products { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        // Condition fields
        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.CategoryIds")]
        public string CategoryIds { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.TopicSystemNames")]
        public string TopicSystemNames { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MickeySalePromo.ShowOnAllPages")]
        public bool ShowOnAllPages { get; set; }
    }
}
