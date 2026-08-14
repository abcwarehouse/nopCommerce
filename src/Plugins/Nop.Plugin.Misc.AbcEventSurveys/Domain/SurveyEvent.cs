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
        /// Custom Terms and Conditions text shown above the consent checkbox. Null/empty falls
        /// back to the standard ABC Warehouse sweepstakes boilerplate.
        /// </summary>
        public string TermsAndConditions { get; set; }

        /// <summary>
        /// Header shown on the thank-you screen after a successful submission. Null/empty falls
        /// back to "Thank You!".
        /// </summary>
        public string ThankYouHeader { get; set; }

        /// <summary>
        /// Description shown on the thank-you screen after a successful submission. Null/empty
        /// falls back to the standard "Your entry has been received" message.
        /// </summary>
        public string ThankYouDescription { get; set; }

        /// <summary>
        /// Whether the page is currently reachable by the public.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// No longer surfaced in the admin UI or enforced by IsEventOpen - retained on the entity/DB
        /// schema only so existing data isn't lost. Do not wire this back up without also restoring
        /// the corresponding admin fields.
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Optional end of the entry window. Also used to fill in the "Promotion ends {date}" line
        /// of the default Terms and Conditions text on the public survey page.
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
