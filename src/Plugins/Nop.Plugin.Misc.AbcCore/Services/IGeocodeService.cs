namespace Nop.Plugin.Misc.AbcCore.Services
{
    public interface IGeocodeService
    {
        Task<(double lat, double lng)> GeocodeZipAsync(string zip);
    }
}