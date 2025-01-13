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

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Controllers
{
    public class List404Controller : BaseAdminController
    {
        public IActionResult List()
        {
            return View(
                "~/Plugins/Misc.AbcCore/Areas/Admin/Views/List404/List.cshtml"
            );
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(List404SearchModel model)
        {
            return null;
        }
    }
}
