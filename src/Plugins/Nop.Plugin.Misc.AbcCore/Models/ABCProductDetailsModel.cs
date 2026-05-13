using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbcCore.Models
{
    public class ABCProductDetailsModel
    {
        public int ProductId { get; set; }

        [NopResourceDisplayName(CoreLocales.PLPDescription)]
        public string PLPDescription { get; set; }

        [NopResourceDisplayName(CoreLocales.AbcShortDescription)]
        public string AbcShortDescription { get; set; }

        [NopResourceDisplayName(CoreLocales.AbcFullDescription)]
        public string AbcFullDescription { get; set; }
    }
}
