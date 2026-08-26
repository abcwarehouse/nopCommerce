using System.Text.Json.Serialization;

namespace AbcWarehouse.Plugin.Widgets.Listrak.Models
{
    /// <summary>
    /// Request body for Listrak's SMS PhoneList Contact API
    /// (POST ShortCode/{ShortCodeId}/PhoneList/{PhoneListId}/Contact).
    /// </summary>
    public class PhoneListContactModel
    {
        public string ShortCodeId { get; set; }

        public string PhoneNumber { get; set; }

        public string PhoneListId { get; set; }

        /// <summary>
        /// Optional. Omitted from the request when null so callers that don't collect an email
        /// send the exact same payload as before.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Optional. Omitted from the request when null so callers that don't collect a name
        /// (e.g. the footer newsletter/SMS signup) send the exact same payload as before.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string FirstName { get; set; }

        /// <summary>
        /// Optional. Omitted from the request when null - see <see cref="FirstName"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string LastName { get; set; }
    }
}
