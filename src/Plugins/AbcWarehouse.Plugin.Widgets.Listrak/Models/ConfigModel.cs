using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace AbcWarehouse.Plugin.Widgets.Listrak.Models
{
    public class ConfigModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Required]
        [NopResourceDisplayName(ListrakLocales.MerchantId)]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [Required]
        [NopResourceDisplayName(ListrakLocales.ClientId)]
        public string ClientId { get; set; }
        public bool ClientId_OverrideForStore { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [NopResourceDisplayName(ListrakLocales.ClientSecret)]
        public string ClientSecret { get; set; }
        public bool ClientSecret_OverrideForStore { get; set; }
    }
}
