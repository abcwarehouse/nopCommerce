using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.AbcHomeDeliveryStatus.Models
{
    public record AbcHomeDeliveryStatusConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbcHomeDeliveryStatus.Fields.UseMockResponses")]
        public bool UseMockResponses { get; set; }
    }
}
