using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    /// <summary>
    /// Page not found record service
    /// </summary>
    public partial class PageNotFoundRecordService : IPageNotFoundRecordService
    {
        #region Fields

        private readonly IRepository<PageNotFoundRecord> _pageNotFoundRecordRepository;

        #endregion

        #region Ctor

        public PageNotFoundRecordService(IRepository<PageNotFoundRecord> pageNotFoundRecordRepository)
        {
            _pageNotFoundRecordRepository = pageNotFoundRecordRepository;
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
        public virtual async Task<IPagedList<PageNotFoundRecord>> GetAllPageNotFoundRecordsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _pageNotFoundRecordRepository.Table
                .OrderByDescending(r => r.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a page not found record by identifier
        /// </summary>
        /// <param name="id">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the page not found record
        /// </returns>
        public virtual async Task<PageNotFoundRecord> GetPageNotFoundRecordByIdAsync(int id)
        {
            return await _pageNotFoundRecordRepository.GetByIdAsync(id);
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

        /// <summary>
        /// Updates a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdatePageNotFoundRecordAsync(PageNotFoundRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await _pageNotFoundRecordRepository.UpdateAsync(record);
        }

        /// <summary>
        /// Deletes a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeletePageNotFoundRecordAsync(PageNotFoundRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await _pageNotFoundRecordRepository.DeleteAsync(record);
        }

        #endregion
    }
}