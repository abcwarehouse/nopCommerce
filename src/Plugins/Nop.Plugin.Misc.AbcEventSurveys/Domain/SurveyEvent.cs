using Nop.Core;

namespace Nop.Plugin.Misc.AbcEventSurveys.Domain
{
    /// <summary>
    /// Represents a single event (tailgate, in-store giveaway, trade show, etc.) that a
    /// survey/sign-up page is generated for. The <see cref="Code"/> is the unique identifier
    /// used both in the public URL (/survey/{code}) and stamped onto every response collected,
    /// so results can be sorted/exported by event.
    /// </summary>
    public class SurveyEvent : BaseEntity
    {
        /// <summary>
        /// Internal name shown in the admin grid.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL-friendly unique identifier for this event, e.g. "pistons-tailgate-2026".
        /// Used in the public route and as the sortable identifier on export.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Large header text displayed at the top of the survey page.
        /// </summary>
        public string Header1 { get; set; }

        /// <summary>
        /// Optional picture displayed just below Header1 on the public survey page. 0 = none.
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Secondary header text displayed below Header1.
        /// </summary>
        public string Header2 { get; set; }

        /// <summary>
        /// Body/description text displayed below the headers and above the form.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether the page is currently reachable by the public.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional window during which the page accepts entries even if IsActive is true.
        /// Null means no restriction on that end.
        /// </summary>
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
