using Nop.Core;

namespace Nop.Plugin.Misc.AbcCore.Domain
{
    public class MickeyLandingPageProductMapping : BaseEntity
    {
        public int MickeyLandingPageId { get; set; }
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
