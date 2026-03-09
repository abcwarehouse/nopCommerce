using Nop.Core.Configuration;
using Nop.Plugin.Widgets.AbcHomeDeliveryStatus.Models;

namespace Nop.Plugin.Widgets.AbcHomeDeliveryStatus
{
    public class AbcHomeDeliveryStatusSettings : ISettings
    {
        public bool UseMockResponses { get; private set; }

        public static AbcHomeDeliveryStatusSettings FromModel(AbcHomeDeliveryStatusConfigurationModel model)
        {
            return new AbcHomeDeliveryStatusSettings()
            {
                UseMockResponses = model.UseMockResponses,
            };
        }

        public AbcHomeDeliveryStatusConfigurationModel ToModel()
        {
            return new AbcHomeDeliveryStatusConfigurationModel
            {
                UseMockResponses = UseMockResponses,
            };
        }

        public static AbcHomeDeliveryStatusSettings Default()
        {
            return new AbcHomeDeliveryStatusSettings()
            {
                UseMockResponses = false
            };
        }
    }
}
