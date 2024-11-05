using Nop.Core.Configuration;
using Nop.Plugin.Misc.AbcCore.Models;
using System.Configuration;
using System.Data.Odbc;

namespace Nop.Plugin.Misc.AbcCore
{
    public class CoreSettings : ISettings
    {
        public bool AreExternalCallsSkipped { get; private set; }
        public bool IsDebugMode { get; private set; }
        public string MobilePhoneNumber { get; private set; }
        public string GoogleMapsGeocodingAPIKey { get; private set; }
        public bool IsFedExMode { get; private set; }
        public string StagingDbConnectionString { get; private set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(GoogleMapsGeocodingAPIKey);
        }

        public static CoreSettings FromModel(ConfigurationModel model)
        {
            return new CoreSettings()
            {
                AreExternalCallsSkipped = model.AreExternalCallsSkipped,
                IsDebugMode = model.IsDebugMode,
                MobilePhoneNumber = model.MobilePhoneNumber,
                GoogleMapsGeocodingAPIKey = model.GoogleMapsGeocodingAPIKey,
                IsFedExMode = model.IsFedExMode,
                StagingDbConnectionString = model.StagingDbConnectionString
            };
        }

        public ConfigurationModel ToModel()
        {
            return new ConfigurationModel
            {
                AreExternalCallsSkipped = AreExternalCallsSkipped,
                IsDebugMode = IsDebugMode,
                MobilePhoneNumber = MobilePhoneNumber,
                GoogleMapsGeocodingAPIKey = GoogleMapsGeocodingAPIKey,
                IsFedExMode = IsFedExMode,
                StagingDbConnectionString = StagingDbConnectionString
            };
        }
    }
}