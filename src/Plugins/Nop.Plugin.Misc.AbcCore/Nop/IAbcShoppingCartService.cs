using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.AbcCore.Nop
{
    public interface IAbcShoppingCartService : IShoppingCartService
    {
        Task<bool> IsCartEligibleForCheckout(IList<ShoppingCartItem> shoppingCart);
    }
}
