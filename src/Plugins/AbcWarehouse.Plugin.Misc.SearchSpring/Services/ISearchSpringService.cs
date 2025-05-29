using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Models;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public interface ISearchSpringService
    {
        Task<SearchResultModel> SearchAsync(string query, string sessionId = null, string userId = null);
    }
}
