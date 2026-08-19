using System;
using System.Text.Json;

namespace AbcWarehouse.Plugin.Widgets.Listrak.Models
{
    /// <summary>
    /// Matches Listrak's SMSContactSubscriptionDetails shape, returned by Get Contact
    /// (GET /v1/ShortCode/{senderCodeId}/Contact/{phoneNumber}) and accepted as the body
    /// of Update Contact (PUT /v1/ShortCode/{senderCodeId}/Contact/{phoneNumber}).
    /// Note: this resource isn't list-scoped (no PhoneListId) - Listrak scopes it by sender
    /// code + phone number only.
    /// </summary>
    public class SmsContactSubscriptionDetails
    {
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthday { get; set; }
        public string PostalCode { get; set; }
        public bool OptedOut { get; set; }

        /// <summary>
        /// Shape isn't documented here - kept opaque so a read-then-write round trip echoes it
        /// back unchanged instead of guessing at (and potentially corrupting) its structure.
        /// </summary>
        public JsonElement? SegmentationFieldValues { get; set; }

        // Response-only, not needed on the Update Contact request body, but harmless to carry.
        public DateTime? SubscribeDate { get; set; }
        public DateTime? UnsubscribeDate { get; set; }
    }

    /// <summary>Envelope for the Get Contact response: { "status": 200, "data": {...} }.</summary>
    public class GetContactResponse
    {
        public int Status { get; set; }
        public SmsContactSubscriptionDetails Data { get; set; }
    }
}
