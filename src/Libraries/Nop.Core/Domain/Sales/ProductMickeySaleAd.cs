using System;

namespace Nop.Core.Domain.Sales
{
    public class ProductMickeySaleAd : BaseEntity
    {
        public int MickeyPromoId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DisplayOrder { get; set; }
    }
}
