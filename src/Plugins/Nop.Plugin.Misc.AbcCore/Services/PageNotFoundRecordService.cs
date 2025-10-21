using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Misc.AbcCore.Domain;
using System.Linq;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    /// <summary>
    /// Page not found record service
    /// </summary>
    public partial class PageNotFoundRecordService : IPageNotFoundRecordService
    {
        #region Fields

        private readonly IRepository<PageNotFoundRecord> _pageNotFoundRecordRepository;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public PageNotFoundRecordService(
            IRepository<PageNotFoundRecord> pageNotFoundRecordRepository,
            ICustomerService customerService)
        {
            _pageNotFoundRecordRepository = pageNotFoundRecordRepository;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all page not found records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paged list of page not found records
        /// </returns>
        public virtual async Task<IPagedList<PageNotFoundRecord>> GetAllPageNotFoundRecordsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            string slug = null,
            string customerEmail = null)
        {
            var query = _pageNotFoundRecordRepository.Table;

            if (!string.IsNullOrWhiteSpace(slug))
            {
                slug = slug.Trim();
                query = query.Where(r => r.Slug.Contains(slug));
            }

            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                var customerId = (await _customerService.GetCustomerByEmailAsync(customerEmail))?.Id;
                if (customerId.HasValue)
                {
                    query = query.Where(p => p.CustomerId == customerId.Value);
                }
                else
                {
                    query = Enumerable.Empty<PageNotFoundRecord>().AsQueryable();
                }
            }

            query = query.OrderByDescending(r => r.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertPageNotFoundRecordAsync(PageNotFoundRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await _pageNotFoundRecordRepository.InsertAsync(record);
        }

        #endregion
    }
}