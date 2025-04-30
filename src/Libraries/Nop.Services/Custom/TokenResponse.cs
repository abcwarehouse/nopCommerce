using Newtonsoft.Json;

namespace Nop.Services.Custom
{
    public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}
    public class ListrakData
    {
        public string ShortCodeId { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneListId { get; set; }
    }
}
