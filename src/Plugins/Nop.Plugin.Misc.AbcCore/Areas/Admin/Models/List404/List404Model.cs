using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcCore.Areas.Admin.Models
{
    public partial record List404Model : BaseNopEntityModel
    {
        public string Slug { get; set; }
        public string ReferrerUrl { get; set; }
    }
}