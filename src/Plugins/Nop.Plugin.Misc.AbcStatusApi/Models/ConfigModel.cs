using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbcStatusApi.Models
{
    public class ConfigModel
    {
        [NopResourceDisplayName(AbcStatusApiLocales.IsDebugModeEnabled)]
        public bool IsDebugModeEnabled { get; set; }
    }
}
