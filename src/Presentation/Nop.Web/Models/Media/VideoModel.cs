using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Media;

public partial record VideoModel : BaseNopModel
{
    public string VideoUrl { get; set; }

    // ABC: added thumbnail URL to support product video thumbnails

    public string ThumbnailUrl { get; set; }

    public string Allow { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}