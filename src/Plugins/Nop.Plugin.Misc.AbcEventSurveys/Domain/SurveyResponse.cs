using Nop.Core;

namespace Nop.Plugin.Misc.AbcEventSurveys.Domain
{
    /// <summary>
    /// A single entrant's submission to a specific <see cref="SurveyEvent"/>.
    /// </summary>
    public class SurveyResponse : BaseEntity
    {
        public int SurveyEventId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        /// <summary>
        /// Whether the entrant checked the box consenting to marketing email/SMS per the
        /// Terms and Conditions shown on the page.
        /// </summary>
        public bool ConsentMarketing { get; set; }

        /// <summary>
        /// Whether the entrant separately opted in to SMS text messages ("Yes, I'd like to
        /// receive text messages from ABC Warehouse"). Gates the Listrak SMS subscribe call.
        /// </summary>
        public bool ConsentSms { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string IpAddress { get; set; }

        /// <summary>
        /// Where the entrant came to the site from (e.g. "google", "listrak", "site"), taken from
        /// the session source the GA4 tag caches in the browser. See Views/Survey/Index.cshtml.
        /// </summary>
        public string TrafficSource { get; set; }

        /// <summary>
        /// Channel the entrant arrived through (e.g. "organic", "email", "popup", "cpc", "direct").
        /// </summary>
        public string TrafficMedium { get; set; }
    }
}
