using System.Threading.Tasks;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public interface ISearchSpringService
    {
        Task<object> SearchAsync(string term); 
    }
}
