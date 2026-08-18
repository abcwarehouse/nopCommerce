using AbcWarehouse.Plugin.Widgets.Listrak.Models;
using Nop.Core.Configuration;

namespace AbcWarehouse.Plugin.Widgets.Listrak
{
    public class ListrakSettings : ISettings
    {
        public string MerchantId { get; set; }

        /// <summary>OAuth2 client_id used to authenticate against Listrak's SMS API.</summary>
        public string ClientId { get; set; }

        /// <summary>OAuth2 client_secret used to authenticate against Listrak's SMS API.</summary>
        public string ClientSecret { get; set; }
    }
}