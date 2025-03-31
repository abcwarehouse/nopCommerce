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
        private readonly IExportManager _exportManager;
        private readonly INotificationService _notificationService;
        private readonly IAbcLogger _logger;
        // temporary
        private readonly ICategoryService _categoryService;

        public PageNotFoundController(
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IExportManager exportManager,
            INotificationService notificationService,
            ICategoryService categoryService,
            IAbcLogger logger)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _exportManager = exportManager;
            _notificationService = notificationService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public virtual async Task<IActionResult> ExportXml()
        {
            try
            {
                var xml = await _exportManager.ExportCategoriesToXmlAsync();

                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        public virtual async Task<IActionResult> ExportXlsx()
        {
            try
            {
                var bytes = await _exportManager
                    .ExportCategoriesToXlsxAsync((await _categoryService.GetAllCategoriesAsync(showHidden: true)).ToList());

                return File(bytes, MimeTypes.TextXlsx, "categories.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
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
            var logs = _logger.GetPageNotFoundLogs();
            if (!string.IsNullOrWhiteSpace(searchModel.Slug))
            {
                logs = logs.Where(log => log.PageUrl.Contains(searchModel.Slug)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(searchModel.CustomerEmail))
            {
                var customerId = (await _customerService.GetCustomerByEmailAsync(searchModel.CustomerEmail))?.Id;
                if (customerId.HasValue)
                {
                    logs = logs.Where(log => log.CustomerId == customerId.Value).ToList();
                }
                else
                {
                    logs = new List<Log>();
                }
            }
            var createdOnFromValue = searchModel.CreatedOnFrom.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()) : null;
            var createdToFromValue = searchModel.CreatedOnTo.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1) : null;

            if (createdOnFromValue != null)
            {
                logs = logs.Where(log => log.CreatedOnUtc >= createdOnFromValue).ToList();
            }
            if (createdToFromValue != null)
            {
                logs = logs.Where(log => log.CreatedOnUtc <= createdToFromValue).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchModel.IpAddress))
            {
                logs = logs.Where(log => log.IpAddress == searchModel.IpAddress).ToList();
            }

            var pagedList = logs.ToPagedList(searchModel);
            var model = await new PageNotFoundListModel().PrepareToGridAsync(searchModel, pagedList, () =>
            {
                //fill in model values from the entity
                return pagedList.SelectAwait(async log =>
                {
                    var customerId = log.CustomerId.HasValue ? log.CustomerId.Value : 0;
                    var PageNotFoundModel = new PageNotFoundModel()
                    {
                        Slug = log.PageUrl,
                        ReferrerUrl = log.ReferrerUrl,
                        CustomerId = customerId,
                        CustomerEmail = (await _customerService.GetCustomerByIdAsync(customerId))?.Email ?? "Guest",
                        Date = await _dateTimeHelper.ConvertToUserTimeAsync(log.CreatedOnUtc, DateTimeKind.Utc),
                        IpAddress = log.IpAddress
                    };

                    return PageNotFoundModel;
                });
            });

            return Json(model);
        }
    }
}
