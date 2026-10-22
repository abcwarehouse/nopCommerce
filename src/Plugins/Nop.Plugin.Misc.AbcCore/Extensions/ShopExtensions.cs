using System;
using SevenSpikes.Nop.Plugins.StoreLocator.Domain.Shops;

namespace Nop.Plugin.Misc.AbcCore.Extensions
{
    public static class ShopExtensions
    {
        private const double EarthRadiusMiles = 3958.8;

        // great-circle (haversine) distance in miles from the shop to the given point
        public static double DistanceInMilesFrom(this Shop shop, double lat, double lng)
        {
            var shopLat = Double.Parse(shop.Latitude);
            var shopLng = Double.Parse(shop.Longitude);

            var dLat = ToRadians(lat - shopLat);
            var dLng = ToRadians(lng - shopLng);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(ToRadians(shopLat)) * Math.Cos(ToRadians(lat)) * Math.Pow(Math.Sin(dLng / 2), 2);
            return EarthRadiusMiles * 2 * Math.Asin(Math.Sqrt(a));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
