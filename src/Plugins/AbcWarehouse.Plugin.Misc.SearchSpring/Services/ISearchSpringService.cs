using System.Threading.Tasks;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Services
{
    public interface ISearchSpringService
    {
        Task<object> SearchAsync(string term); // Replace `object` with a proper model when available
    }
}
