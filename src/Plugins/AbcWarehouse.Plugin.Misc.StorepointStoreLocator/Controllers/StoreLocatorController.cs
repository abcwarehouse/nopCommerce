using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace AbcWarehouse.Plugin.Misc.StorepointStoreLocator.Controllers
{
    public class StoreLocatorController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // ABC: our exact URL is here.
        private const string STOREPOINT_SCRIPT_URL = "https://cdn.storepoint.co/api/v1/js/15faae969dbf06.js";
        private const string STOREPOINT_MAP_ID = "15faae969dbf06";

        public StoreLocatorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["StorepointScriptUrl"] = STOREPOINT_SCRIPT_URL;
            ViewData["StorepointLocations"] = await LoadStorepointLocationsAsync();

            return View("~/Plugins/Misc.StorepointStoreLocator/Views/Index.cshtml");
        }

        private async Task<IList<StorepointLocation>> LoadStorepointLocationsAsync()
        {
            var locations = new List<StorepointLocation>();

            if (string.IsNullOrWhiteSpace(STOREPOINT_MAP_ID))
                return locations;

            var endpoint = $"https://api.storepoint.co/v1/{STOREPOINT_MAP_ID}/locations?rq";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync(endpoint);

                using var document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty("results", out var results))
                    return locations;

                if (!results.TryGetProperty("locations", out var items) || items.ValueKind != JsonValueKind.Array)
                    return locations;

                foreach (var item in items.EnumerateArray())
                {
                    locations.Add(new StorepointLocation
                    {
                        Id = GetString(item, "id"),
                        Name = GetString(item, "name"),
                        StreetAddress = GetString(item, "streetaddress", "address", "street_address"),
                        City = GetString(item, "city"),
                        State = GetString(item, "state", "province", "region"),
                        PostalCode = GetString(item, "postal", "zipcode", "zip"),
                        Phone = GetString(item, "phone"),
                        Website = GetString(item, "website")
                    });
                }
            }
            catch
            {
                // Keep the page render resilient if Storepoint is temporarily unavailable.
            }

            return locations;
        }

        private static string GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var value))
                    continue;

                if (value.ValueKind == JsonValueKind.String)
                    return value.GetString();

                if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                    return value.ToString();
            }

            return string.Empty;
        }

        public class StorepointLocation
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Phone { get; set; }
            public string Website { get; set; }
        }
    }
}
