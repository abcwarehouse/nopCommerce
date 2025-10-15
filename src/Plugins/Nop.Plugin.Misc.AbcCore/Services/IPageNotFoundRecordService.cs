using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    /// <summary>
    /// Page not found record service interface
    /// </summary>
    public partial interface IPageNotFoundRecordService
    {
        /// <summary>
        /// Gets all page not found records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paged list of page not found records
        /// </returns>
        Task<IPagedList<PageNotFoundRecord>> GetAllPageNotFoundRecordsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a page not found record by identifier
        /// </summary>
        /// <param name="id">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the page not found record
        /// </returns>
        Task<PageNotFoundRecord> GetPageNotFoundRecordByIdAsync(int id);

        /// <summary>
        /// Inserts a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertPageNotFoundRecordAsync(PageNotFoundRecord record);

        /// <summary>
        /// Updates a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdatePageNotFoundRecordAsync(PageNotFoundRecord record);

        /// <summary>
        /// Deletes a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeletePageNotFoundRecordAsync(PageNotFoundRecord record);
    }
}
