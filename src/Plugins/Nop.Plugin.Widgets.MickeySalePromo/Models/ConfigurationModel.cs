using Microsoft.AspNetCore.Http;
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

        public List<SaleProductModel> Products { get; set; }
    }
}
