namespace Nop.Services.Custom
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
    public class ListrakData
    {
        public string ShortCodeId { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneListId { get; set; }
    }
}