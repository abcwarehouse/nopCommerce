using Nop.Core;

namespace Nop.Plugin.Misc.AbcEventSurveys.Domain
{
    /// <summary>
    /// A single admin-defined extra text field attached to a <see cref="SurveyEvent"/>,
    /// e.g. "T-Shirt Size" or "Favorite Product". Rendered as an additional text input
    /// on the public survey page for that event only.
    /// </summary>
    public class SurveyCustomField : BaseEntity
    {
        public int SurveyEventId { get; set; }

        /// <summary>
        /// Label shown to the entrant.
        /// </summary>
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }
    }
}
