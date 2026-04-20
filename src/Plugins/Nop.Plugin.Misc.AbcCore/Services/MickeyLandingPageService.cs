using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public class MickeyLandingPageService : IMickeyLandingPageService
    {
        private readonly IRepository<MickeyLandingPage> _landingPageRepository;
        private readonly IRepository<MickeyLandingPageProductMapping> _mappingRepository;
        private readonly IProductService _productService;

        public MickeyLandingPageService(
            IRepository<MickeyLandingPage> landingPageRepository,
            IRepository<MickeyLandingPageProductMapping> mappingRepository,
            IProductService productService)
        {
            _landingPageRepository = landingPageRepository;
            _mappingRepository = mappingRepository;
            _productService = productService;
        }

        public async Task<IList<MickeyLandingPage>> GetAllLandingPagesAsync()
        {
            return await _landingPageRepository.Table
                .OrderByDescending(lp => lp.StartDate)
                .ToListAsync();
        }

        public async Task<MickeyLandingPage> GetLandingPageByIdAsync(int id)
        {
            return await _landingPageRepository.GetByIdAsync(id);
        }

        public async Task<MickeyLandingPage> GetActiveLandingPageAsync()
        {
            var all = await GetAllLandingPagesAsync();
            return all.FirstOrDefault(lp => lp.IsActive());
        }

        public async Task InsertLandingPageAsync(MickeyLandingPage landingPage)
        {
            if (landingPage == null)
                throw new ArgumentNullException(nameof(landingPage));
            await _landingPageRepository.InsertAsync(landingPage);
        }

        public async Task UpdateLandingPageAsync(MickeyLandingPage landingPage)
        {
            if (landingPage == null)
                throw new ArgumentNullException(nameof(landingPage));
            await _landingPageRepository.UpdateAsync(landingPage);
        }

        public async Task DeleteLandingPageAsync(MickeyLandingPage landingPage)
        {
            if (landingPage == null)
                throw new ArgumentNullException(nameof(landingPage));

            // Remove all product mappings first
            var mappings = await GetMappingsByLandingPageIdAsync(landingPage.Id);
            foreach (var mapping in mappings)
                await _mappingRepository.DeleteAsync(mapping);

            await _landingPageRepository.DeleteAsync(landingPage);
        }

        public async Task<IList<MickeyLandingPageProductMapping>> GetMappingsByLandingPageIdAsync(int landingPageId)
        {
            return await _mappingRepository.Table
                .Where(m => m.MickeyLandingPageId == landingPageId)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<IList<MickeyLandingPageProductMapping>> GetMappingsByProductIdAsync(int productId)
        {
            return await _mappingRepository.Table
                .Where(m => m.ProductId == productId)
                .ToListAsync();
        }

        public async Task<MickeyLandingPageProductMapping> GetMappingByIdAsync(int mappingId)
        {
            return await _mappingRepository.GetByIdAsync(mappingId);
        }

        public async Task InsertMappingAsync(MickeyLandingPageProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            await _mappingRepository.InsertAsync(mapping);
        }

        public async Task DeleteMappingAsync(MickeyLandingPageProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            await _mappingRepository.DeleteAsync(mapping);
        }

        public async Task<IList<Product>> GetProductsByLandingPageIdAsync(int landingPageId)
        {
            var mappings = await GetMappingsByLandingPageIdAsync(landingPageId);
            var productIds = mappings.Select(m => m.ProductId).ToArray();
            if (!productIds.Any())
                return new List<Product>();
            return await _productService.GetProductsByIdsAsync(productIds);
        }
    }
}
