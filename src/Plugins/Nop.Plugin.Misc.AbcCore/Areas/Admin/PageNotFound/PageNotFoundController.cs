using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Models.Extensions;
using System.Linq;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Services.Seo;
using System.Threading.Tasks;
using Nop.Plugin.Misc.AbcCore.Areas.Admin.Models;
using Nop.Plugin.Misc.AbcCore.Services.Custom;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Services.Logging;
using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Plugin.Misc.AbcCore.Nop;
using Nop.Services.Helpers;
using System;
using Nop.Services.Customers;
using System.Collections.Generic;
using Nop.Core.Domain.Logging;
using Nop.Services.ExportImport;
using Nop.Services.Messages;
using System.Text;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.PageNotFound
{
    public class PageNotFoundController : BaseAdminController
    {
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAbcExportManager _exportManager;
        private readonly INotificationService _notificationService;
        private readonly IPageNotFoundRecordService _pageNotFoundRecordService;

        public PageNotFoundController(
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IAbcExportManager exportManager,
            INotificationService notificationService,
            IPageNotFoundRecordService pageNotFoundRecordService)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _exportManager = exportManager;
            _notificationService = notificationService;
            _pageNotFoundRecordService = pageNotFoundRecordService;
        }

        public virtual async Task<IActionResult> ExportXlsx()
        {
            // try
            // {
            //     var logs = _logger.GetPageNotFoundLogs();
            //     var bytes = await _exportManager
            //         .ExportPageNotFoundRecordsToXlsxAsync(logs);

            //     return File(bytes, MimeTypes.TextXlsx, "page-not-found-records.xlsx");
            // }
            // catch (Exception exc)
            // {
            //     await _notificationService.ErrorNotificationAsync(exc);
            //     return RedirectToAction("List");
            // }

            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/PageNotFound/List.cshtml",
                new PageNotFoundSearchModel()
            );
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(PageNotFoundSearchModel searchModel)
        {
            var pageNotFoundRecords = await _pageNotFoundRecordService.GetAllPageNotFoundRecordsAsync();
            // if (!string.IsNullOrWhiteSpace(searchModel.Slug))
            // {
            //     logs = logs.Where(log => log.PageUrl.Contains(searchModel.Slug)).ToList();
            // }
            // if (!string.IsNullOrWhiteSpace(searchModel.CustomerEmail))
            // {
            //     var customerId = (await _customerService.GetCustomerByEmailAsync(searchModel.CustomerEmail))?.Id;
            //     if (customerId.HasValue)
            //     {
            //         logs = logs.Where(log => log.CustomerId == customerId.Value).ToList();
            //     }
            //     else
            //     {
            //         logs = new List<Log>();
            //     }
            // }
            // var createdOnFromValue = searchModel.CreatedOnFrom.HasValue
            //     ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()) : null;
            // var createdToFromValue = searchModel.CreatedOnTo.HasValue
            //     ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1) : null;

            // if (createdOnFromValue != null)
            // {
            //     logs = logs.Where(log => log.CreatedOnUtc >= createdOnFromValue).ToList();
            // }
            // if (createdToFromValue != null)
            // {
            //     logs = logs.Where(log => log.CreatedOnUtc <= createdToFromValue).ToList();
            // }

            // if (!string.IsNullOrWhiteSpace(searchModel.IpAddress))
            // {
            //     logs = logs.Where(log => log.IpAddress == searchModel.IpAddress).ToList();
            // }

            var pagedList = pageNotFoundRecords.ToPagedList(searchModel);
            var model = await new PageNotFoundListModel().PrepareToGridAsync(searchModel, pagedList, () =>
            {
                //fill in model values from the entity
                return pagedList.SelectAwait(async pageNotFoundRecord =>
                {
                    var PageNotFoundModel = new PageNotFoundModel()
                    {
                        Slug = pageNotFoundRecord.Slug,
                        ReferrerUrl = pageNotFoundRecord.Referrer,
                        CustomerId = pageNotFoundRecord.CustomerId,
                        CustomerEmail = (await _customerService.GetCustomerByIdAsync(pageNotFoundRecord.CustomerId))?.Email ?? "Guest",
                        Date = await _dateTimeHelper.ConvertToUserTimeAsync(pageNotFoundRecord.CreatedOnUtc, DateTimeKind.Utc),
                        IpAddress = pageNotFoundRecord.IpAddress
                    };

                    return PageNotFoundModel;
                });
            });

            return Json(model);
        }

        public IActionResult Frequency()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/PageNotFound/Frequency.cshtml",
                new PageNotFoundFreqSearchModel()
            );
        }

        [HttpPost]
        public virtual IActionResult Frequency(PageNotFoundFreqSearchModel searchModel)
        {
            // var logs = _logger.GetPageNotFoundLogs();
            // var groupedLogs = logs.GroupBy(l => l.PageUrl)
            //                       .Select(group => new
            //                       {
            //                           Slug = group.Key,
            //                           Count = group.Count()
            //                       })
            //                       .OrderByDescending(g => g.Count)
            //                       .ToList();

            // var pagedList = groupedLogs.ToPagedList(searchModel);
            // var model = new PageNotFoundFreqListModel().PrepareToGrid(searchModel, pagedList, () =>
            // {
            //     //fill in model values from the entity
            //     return pagedList.Select(group =>
            //     {
            //         var pageNotFoundFreqModel = new PageNotFoundFreqModel()
            //         {
            //             Slug = group.Slug,
            //             Count = group.Count
            //         };

            //         return pageNotFoundFreqModel;
            //     });
            // });

            // return Json(model);
            return null;
        }
    }
}
