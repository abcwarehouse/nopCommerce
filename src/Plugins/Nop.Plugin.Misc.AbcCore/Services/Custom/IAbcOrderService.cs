using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AbcCore.Services.Custom
{
    public interface IAbcOrderService : IOrderService
    {
        IList<Order> GetUnsubmittedOrders();

        Task<ProductAttributeValue> GetOrderItemWarrantyAsync(OrderItem orderItem);
    }
}
