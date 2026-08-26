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

        /// <summary>
        /// Sender code ID used by Listrak's Contact API (Get/Update Contact -
        /// /ShortCode/{senderCodeId}/Contact/{phoneNumber}). NOT the same ID as the ShortCodeId
        /// used for the PhoneList Subscribe endpoint, despite the similar naming - confirmed by a
        /// live 404 (ERROR_UNABLE_TO_LOCATE_RESOURCE) when the PhoneList ShortCodeId was reused
        /// here. Get this value from Listrak's dashboard/account team. Enrichment (filling in a
        /// missing name/email on an already-subscribed contact) is skipped, not attempted, while
        /// this is blank - see ListrakService.SubscribeOrEnrichPhoneNumberAsync.
        /// </summary>
        public string SenderCodeId { get; set; }
    }
}