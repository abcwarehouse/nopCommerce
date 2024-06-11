using Nop.Core.Domain.Catalog;
using System;

namespace Nop.Web.Models
{
    public class SaleAdModel
    {
        public int SaleId { get; set; }
        public Product Product { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DisplayOrder { get; set; }
    }
}
