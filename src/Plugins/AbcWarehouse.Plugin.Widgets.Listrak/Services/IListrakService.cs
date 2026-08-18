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
    Task<HttpResponseMessage> SubscribePhoneNumberAsync(string phoneNumber, string firstName = null, string lastName = null);
}