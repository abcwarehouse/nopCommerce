using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Sales;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class SaleAdController : BasePublicController
    {
        private readonly IProductService _productService;
        private readonly IProductMickeySaleAdService _productMickeySaleAdService;

        public SaleAdController(IProductService productService, IProductMickeySaleAdService productMickeySaleAdService)
        {
            _productService = productService;
            _productMickeySaleAdService = productMickeySaleAdService;
        }

        public async Task<IActionResult> Index()
        {
            var sales = _productMickeySaleAdService.GetAllSaleAds();

            var model = new List<SaleAdModel>();

            foreach (var sale in sales)
            {
                var product = await _productService.GetProductByIdAsync(sale.ProductId);
                model.Add(new SaleAdModel
                {
                    SaleId = sale.MickeyPromoId,
                    Product = product,
                    StartDate = sale.StartDate,
                    EndDate = sale.EndDate,
                    DisplayOrder = sale.DisplayOrder
                });
            }

            return View(model);
        }
    }
}
