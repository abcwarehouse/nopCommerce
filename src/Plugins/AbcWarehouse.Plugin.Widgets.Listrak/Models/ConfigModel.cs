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

        // Deliberately not [Required]: the password editor always renders this field blank on
        // load regardless of the saved value (browser security convention), so requiring it would
        // force re-entering the secret on every single save of this form, for any field. A blank
        // submission is treated as "leave the saved value unchanged" - see ListrakController.
        [DataType(DataType.Password)]
        [NopResourceDisplayName(ListrakLocales.ClientSecret)]
        public string ClientSecret { get; set; }
        public bool ClientSecret_OverrideForStore { get; set; }

        // Deliberately not [Required]: enrichment (filling in a missing name/email on an
        // already-subscribed contact) is skipped gracefully while this is blank, rather than the
        // whole Configure form being unsavable until the right ID is tracked down.
        [NopResourceDisplayName(ListrakLocales.SenderCodeId)]
        public string SenderCodeId { get; set; }
        public bool SenderCodeId_OverrideForStore { get; set; }
    }
}
