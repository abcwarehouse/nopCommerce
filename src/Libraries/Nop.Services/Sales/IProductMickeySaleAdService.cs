using Nop.Core.Domain.Sales;
using System.Collections.Generic;

namespace Nop.Services.Sales
{
    public interface IProductMickeySaleAdService
    {
        IEnumerable<ProductMickeySaleAd> GetAllSaleAds();
    }
}
