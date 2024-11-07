using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AbcCore.ProductVideo
{
    public partial class ProductVideo : BaseEntity
    {
        public int ProductId { get; set; }
        public int PictureId { get; set; }
        public string VideoUrl { get; set; }
    }
}