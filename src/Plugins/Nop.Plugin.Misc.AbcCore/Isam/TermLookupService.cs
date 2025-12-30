using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public class TermLookupService : ITermLookupService
    {
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly CoreSettings _settings;
        private readonly IProductAttributeParser _productAttributeParser;

        public TermLookupService(
            IProductService productService,
            ILogger logger,
            CoreSettings settings,
            IProductAttributeParser productAttributeParser
        )
        {
            _productService = productService;
            _logger = logger;
            _settings = settings;
            _productAttributeParser = productAttributeParser;
        }

        public async Task<(string termNo, string description, string link)> GetTermAsync(
            IList<ShoppingCartItem> cart
        )
        {
            if (_settings.AreExternalCallsSkipped)
            {
                await _logger.WarningAsync("External calls are turned off, term lookup skipped.");
                return (null, null, null);
            }

            string nl = Environment.NewLine;
            string xml = "";
            xml = $"<Request>{nl}<Term_Lookup>{nl}<Items>";
            foreach (var item in cart)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var priceAdjustment = (await _productAttributeParser.ParseProductAttributeValuesAsync(item.AttributesXml))
                                                            .Sum(p => p.PriceAdjustment);
                var price = product.Price + priceAdjustment;
                xml += $"{nl}<Item>{nl}<Sku>{product.Sku}</Sku>{nl}" +
                       $"<Gtin>{product.Gtin}</Gtin>{nl}" +
                       $"<Qty>{item.Quantity}</Qty>{nl}" +
                       $"<Brand>{product.Name}</Brand>{nl}" +
                       $"<Price>{price}</Price>{nl}</Item>";
            }
            xml += $"{nl}</Items>{nl}</Term_Lookup>{nl}</Request>";

            if (_settings.IsDebugMode)
            {
                await _logger.InsertLogAsync(
                    LogLevel.Information,
                    "Term Lookup Request",
                    xml
                );
            }

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var content = new System.Net.Http.StringContent(xml, Encoding.UTF8, "text/xml");
                System.Net.Http.HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsync(AbcConstants.StatusAPIUrl, content);
                    response.EnsureSuccessStatusCode();
                }
                catch (System.Net.Http.HttpRequestException e)
                {
                    throw new IsamException($"Error when connecting to Term Lookup API: {e.Message}");
                }

                string strResponse = await response.Content.ReadAsStringAsync();

                if (_settings.IsDebugMode)
                {
                    await _logger.InsertLogAsync(
                        LogLevel.Information,
                        "Term Lookup Response",
                        strResponse
                    );
                }

                if (string.IsNullOrEmpty(strResponse))
                {
                    throw new IsamException(
                        "Empty response received from ISAM backend during Term Lookup."
                    );
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strResponse);

                var termNo = xmlDoc.SelectSingleNode("Response/Term_Lookup/Term_No");
                var description = xmlDoc.SelectSingleNode("Response/Term_Lookup/Description");
                var link = xmlDoc.SelectSingleNode("Response/Term_Lookup/Link");

                return (termNo?.InnerText, description?.InnerText, link?.InnerText);
            }
        }
    }
}