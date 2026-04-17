using System.Linq;
using Nop.Core.Domain.Catalog;
using System.IO;
using Nop.Data;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Nop.Services.Media;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Plugin.Misc.AbcCore.Extensions;
using System.Collections.Generic;
using Nop.Plugin.Misc.AbcCore.Services.Custom;
using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;
using ClosedXML.Excel;

namespace Nop.Plugin.Misc.AbcCore
{
    class MissingImageReportTask : IScheduleTask
    {
        private readonly string _excelPath;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductAbcDescription> _productAbcRepository;
        private readonly IPictureService _pictureService;
        private readonly IAbcProductService _customProductService;

        private static class ProductTable
        {
            public static int IdIdx = 0;
            public static int NameIdx = 1;
            public static int SkuIdx = 2;
            public static int ItemNoIdx = 3;
        }

        public MissingImageReportTask(
            IRepository<Product> productRepository,
            IRepository<ProductAbcDescription> productAbcRepository,
            IPictureService pictureService,
            IAbcProductService customProductService
        )
        {
            _productRepository = productRepository;
            _productAbcRepository = productAbcRepository;
            _pictureService = pictureService;
            _customProductService = customProductService;

            var env = EngineContext.Current.Resolve<IWebHostEnvironment>();
            _excelPath = Path.Combine(env.WebRootPath, "ImageReport.xlsx");
        }

        public async Task ExecuteAsync()
        {
            var prodsInfo = from prod in await _customProductService.GetProductsWithoutImagesAsync()
                            from pAbc in _productAbcRepository.Table.Where(pA => pA.Product_Id == prod.Id).ToList()
                            select new
                            {
                                prod.Id,
                                prod.Name,
                                prod.Sku,
                                ItemNo = pAbc == null ? "" : pAbc.AbcItemNumber
                            };

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Missing Images");
            int rowIdx = 2;

            worksheet.Cell(1, ProductTable.IdIdx + 1).Value = "Id";
            worksheet.Cell(1, ProductTable.NameIdx + 1).Value = "Name";
            worksheet.Cell(1, ProductTable.SkuIdx + 1).Value = "Sku";
            worksheet.Cell(1, ProductTable.ItemNoIdx + 1).Value = "Item No";

            foreach (var prodInfo in prodsInfo)
            {
                worksheet.Cell(rowIdx, ProductTable.IdIdx + 1).Value = prodInfo.Id;
                worksheet.Cell(rowIdx, ProductTable.NameIdx + 1).Value = prodInfo.Name;
                worksheet.Cell(rowIdx, ProductTable.SkuIdx + 1).Value = prodInfo.Sku;
                worksheet.Cell(rowIdx, ProductTable.ItemNoIdx + 1).Value = prodInfo.ItemNo;
                ++rowIdx;
            }
            workbook.SaveAs(_excelPath);
        }
    }
}