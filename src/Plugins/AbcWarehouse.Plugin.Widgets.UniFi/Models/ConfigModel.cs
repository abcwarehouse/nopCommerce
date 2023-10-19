using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace AbcWarehouse.Plugin.Widgets.UniFi.Models
{
    public class ConfigModel
    {
        [Required]
        [NopResourceDisplayName(UniFiLocales.PartnerId)]
        public string PartnerId { get; set; }
    }
}