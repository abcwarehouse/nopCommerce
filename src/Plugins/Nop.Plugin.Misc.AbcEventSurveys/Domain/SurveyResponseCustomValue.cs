using Nop.Core;

namespace Nop.Plugin.Misc.AbcEventSurveys.Domain
{
    /// <summary>
    /// The value an entrant supplied for one of the event's <see cref="SurveyCustomField"/>s.
    /// </summary>
    public class SurveyResponseCustomValue : BaseEntity
    {
        public int SurveyResponseId { get; set; }
        public int SurveyCustomFieldId { get; set; }
        public string Value { get; set; }
    }
}
