namespace AbcWarehouse.Plugin.Widgets.Listrak.Models
{
    /// <summary>
    /// Shape of Listrak's SMS API error responses (400/401/404), e.g.
    /// { "status": 400, "error": "ERROR_PHONE_NUMBER_FOUND", "message": "..." }.
    /// </summary>
    public class ListrakApiErrorResponse
    {
        public int Status { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
