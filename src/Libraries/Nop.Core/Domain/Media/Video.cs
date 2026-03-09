namespace Nop.Core.Domain.Media;

/// <summary>
/// Represents a video
/// </summary>
public partial class Video : BaseEntity
{
    /// <summary>
    /// Gets or sets the URL of video
    /// </summary>
    public string VideoUrl { get; set; }

    // ABC: added thumbnail URL to support product video thumbnails
    public string ThumbnailUrl { get; set; }
}