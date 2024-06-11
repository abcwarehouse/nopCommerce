using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Sales;
using Nop.Data;

namespace Nop.Services.Sales
{
    public class ProductMickeySaleAdService : IProductMickeySaleAdService
    {
        private readonly IRepository<ProductMickeySaleAd> _saleAdRepository;

        public ProductMickeySaleAdService(IRepository<ProductMickeySaleAd> saleAdRepository)
        {
            _saleAdRepository = saleAdRepository;
        }

        public IEnumerable<ProductMickeySaleAd> GetAllSaleAds()
        {
            return _saleAdRepository.Table.ToList();
        }
    }
}
