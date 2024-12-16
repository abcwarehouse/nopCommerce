using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Services.Common;
using SevenSpikes.Nop.Plugins.StoreLocator.Domain.Shops;
using SevenSpikes.Nop.Plugins.StoreLocator.Services;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public interface ICustomShopService : IShopService
    {
        Shop GetShopByAbcBranchId(string branchId);

        Shop GetShopByName(string name);

        ShopAbc GetShopAbcByShopId(int shopId);
        Task<string> GetBearerTokenAsync(IGenericAttributeService genericAttributeService, IWorkContext workContext);
        Task<bool> IsTokenExpiredAsync(IWorkContext workContext, IGenericAttributeService genericAttributeService);
        Task<string> GetBearerTokenAsync();
        Task<bool> IsTokenExpiredAsync();
    }
}