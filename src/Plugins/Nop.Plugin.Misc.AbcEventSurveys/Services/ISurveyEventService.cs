using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Services
{
    public interface ISurveyEventService
    {
        Task<IList<SurveyEvent>> GetAllEventsAsync();
        Task<SurveyEvent> GetEventByIdAsync(int id);
        Task<SurveyEvent> GetEventByCodeAsync(string code);

        /// <summary>
        /// True if the event is active, published within its optional date window, and
        /// therefore reachable by the public.
        /// </summary>
        bool IsEventOpen(SurveyEvent surveyEvent);

        Task InsertEventAsync(SurveyEvent surveyEvent);
        Task UpdateEventAsync(SurveyEvent surveyEvent);
        Task DeleteEventAsync(SurveyEvent surveyEvent);

        /// <summary>
        /// Generates a unique, URL-friendly code from a display name (e.g. "Pistons Tailgate 2026"
        /// -> "pistons-tailgate-2026", appending "-2" etc. if already taken).
        /// </summary>
        Task<string> GenerateUniqueCodeAsync(string name);

        Task<IList<SurveyCustomField>> GetCustomFieldsByEventIdAsync(int surveyEventId);
        Task<SurveyCustomField> GetCustomFieldByIdAsync(int id);
        Task InsertCustomFieldAsync(SurveyCustomField customField);
        Task DeleteCustomFieldAsync(SurveyCustomField customField);

        Task<IList<SurveyResponse>> GetResponsesByEventIdAsync(int surveyEventId);
        Task<int> GetResponseCountByEventIdAsync(int surveyEventId);
        Task<IList<SurveyResponseCustomValue>> GetCustomValuesByResponseIdAsync(int surveyResponseId);
        Task<IDictionary<int, IList<SurveyResponseCustomValue>>> GetCustomValuesByEventIdAsync(int surveyEventId);

        /// <summary>
        /// Saves a new entrant's response along with the custom field values they supplied
        /// (keyed by SurveyCustomFieldId).
        /// </summary>
        Task InsertResponseAsync(SurveyResponse response, IDictionary<int, string> customFieldValues);
    }
}
