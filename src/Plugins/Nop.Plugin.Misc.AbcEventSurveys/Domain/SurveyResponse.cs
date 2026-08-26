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

        public DateTime CreatedOnUtc { get; set; }

        public string IpAddress { get; set; }
    }
}
