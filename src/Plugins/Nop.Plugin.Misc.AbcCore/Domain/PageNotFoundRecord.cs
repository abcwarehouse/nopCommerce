using Nop.Core;
using Nop.Data;
using System;
using System.Linq;

namespace Nop.Plugin.Misc.AbcCore.Domain
{
    public partial class PageNotFoundRecord : BaseEntity
    {
        public virtual string Slug { get; set; }
        public virtual string Referrer { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string IpAddress { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
    }
}