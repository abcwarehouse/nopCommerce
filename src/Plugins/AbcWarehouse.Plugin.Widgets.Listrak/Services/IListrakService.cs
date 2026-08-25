using System.Net.Http;
using System.Threading.Tasks;

public interface IListrakService
{
    Task<string> GetTokenAsync();

    /// <summary>
    /// Adds a phone number to the configured Listrak SMS phone list.
    /// </summary>
    /// <param name="firstName">Optional. Leave null for callers that don't collect a name (e.g. the footer signup).</param>
    /// <param name="lastName">Optional. Leave null for callers that don't collect a name (e.g. the footer signup).</param>
    /// <param name="emailAddress">Optional. Leave null for callers that don't collect an email.</param>
    Task<HttpResponseMessage> SubscribePhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null, string emailAddress = null);

    /// <summary>
    /// Subscribes a phone number like <see cref="SubscribePhoneNumberAsync"/>. If the number is
    /// already subscribed, this additionally fills in FirstName/LastName/EmailAddress on the
    /// existing Listrak contact - but only fields that are currently blank; it never overwrites
    /// data Listrak already has. Returns the original subscribe attempt's response either way, so
    /// callers can keep treating it exactly like <see cref="SubscribePhoneNumberAsync"/> for
    /// status/error handling.
    /// </summary>
    Task<HttpResponseMessage> SubscribeOrEnrichPhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null, string emailAddress = null);
}