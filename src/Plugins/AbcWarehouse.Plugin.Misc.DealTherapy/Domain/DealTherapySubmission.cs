using System;
using Nop.Core;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Domain
{
    public class DealTherapySubmission : BaseEntity
    {
        public int? CustomerId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
