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
        Task<SurveyResponse> GetResponseByIdAsync(int id);
        Task<int> GetResponseCountByEventIdAsync(int surveyEventId);

        /// <summary>
        /// True if this email address has submitted a response (to any event, not just this one)
        /// at or after <paramref name="sinceUtc"/>. Used to enforce a rolling submission-frequency
        /// limit per email address (see SurveyController) - matched case-insensitively.
        /// </summary>
        Task<bool> HasRecentResponseWithEmailAsync(string email, DateTime sinceUtc);
        Task<IList<SurveyResponseCustomValue>> GetCustomValuesByResponseIdAsync(int surveyResponseId);
        Task<IDictionary<int, IList<SurveyResponseCustomValue>>> GetCustomValuesByEventIdAsync(int surveyEventId);

        /// <summary>
        /// Saves a new entrant's response along with the custom field values they supplied
        /// (keyed by SurveyCustomFieldId).
        /// </summary>
        Task InsertResponseAsync(SurveyResponse response, IDictionary<int, string> customFieldValues);

        /// <summary>
        /// Deletes a single response (and its custom field answers, which cascade-delete at the
        /// DB level - see SchemaMigration). This only removes the local record; it doesn't touch
        /// any Listrak contact that was created/enriched from it.
        /// </summary>
        Task DeleteResponseAsync(SurveyResponse response);
    }
}
