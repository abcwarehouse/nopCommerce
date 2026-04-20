using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AbcCore.Domain;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public interface IMickeyLandingPageService
    {
        // Landing page CRUD
        Task<IList<MickeyLandingPage>> GetAllLandingPagesAsync();
        Task<MickeyLandingPage> GetLandingPageByIdAsync(int id);
        Task<MickeyLandingPage> GetActiveLandingPageAsync();
        Task InsertLandingPageAsync(MickeyLandingPage landingPage);
        Task UpdateLandingPageAsync(MickeyLandingPage landingPage);
        Task DeleteLandingPageAsync(MickeyLandingPage landingPage);

        // Product mapping
        Task<IList<MickeyLandingPageProductMapping>> GetMappingsByLandingPageIdAsync(int landingPageId);
        Task<IList<MickeyLandingPageProductMapping>> GetMappingsByProductIdAsync(int productId);
        Task<MickeyLandingPageProductMapping> GetMappingByIdAsync(int mappingId);
        Task InsertMappingAsync(MickeyLandingPageProductMapping mapping);
        Task DeleteMappingAsync(MickeyLandingPageProductMapping mapping);

        // Products for a landing page
        Task<IList<Product>> GetProductsByLandingPageIdAsync(int landingPageId);
    }
}
