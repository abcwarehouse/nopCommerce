using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Misc.AbcCore.Domain;

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
        Task<IPagedList<PageNotFoundRecord>> GetAllPageNotFoundRecordsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            string slug = null,
            string customerEmail = null);

        /// <summary>
        /// Inserts a page not found record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertPageNotFoundRecordAsync(PageNotFoundRecord record);
    }
}
