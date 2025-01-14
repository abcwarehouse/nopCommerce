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

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Controllers
{
    public class List404Controller : BaseAdminController
    {
        private readonly ILogger _logger;

        public List404Controller(ILogger logger)
        {
            _logger = logger;
        }

        public IActionResult List()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/List404/List.cshtml",
                new List404SearchModel()
            );
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(List404SearchModel searchModel)
        {
            var logs = await _logger.GetAllLogsAsync(
                null, null, "Error 404."
            );
            var pagedList = logs.ToPagedList(searchModel);
            var model = new List404ListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                //fill in model values from the entity
                return pagedList.Select(log =>
                {
                    var insideParenthesesPattern = @"\((.*?)\)";
                    var match = Regex.Match(log.ShortMessage, insideParenthesesPattern);
                    string slug = null;
                    if (match.Success)
                    {
                        slug = match.Groups[1].Value;
                    }
                    else
                    {
                        throw new NopException("Malformed 404 log.");
                    }

                    var list404Model = new List404Model()
                    {
                        Slug = slug,
                        ReferrerUrl = log.ReferrerUrl
                    };

                    return list404Model;
                });
            });

            return Json(model);
        }
    }
}
