using Nop.Data;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Services
{
    public class SurveyEventService : ISurveyEventService
    {
        private readonly IRepository<SurveyEvent> _surveyEventRepository;
        private readonly IRepository<SurveyCustomField> _surveyCustomFieldRepository;
        private readonly IRepository<SurveyResponse> _surveyResponseRepository;
        private readonly IRepository<SurveyResponseCustomValue> _surveyResponseCustomValueRepository;

        public SurveyEventService(
            IRepository<SurveyEvent> surveyEventRepository,
            IRepository<SurveyCustomField> surveyCustomFieldRepository,
            IRepository<SurveyResponse> surveyResponseRepository,
            IRepository<SurveyResponseCustomValue> surveyResponseCustomValueRepository)
        {
            _surveyEventRepository = surveyEventRepository;
            _surveyCustomFieldRepository = surveyCustomFieldRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _surveyResponseCustomValueRepository = surveyResponseCustomValueRepository;
        }

        public async Task<IList<SurveyEvent>> GetAllEventsAsync()
        {
            var events = await _surveyEventRepository.GetAllAsync(query => query.OrderByDescending(e => e.CreatedOnUtc));
            return events;
        }

        public async Task<SurveyEvent> GetEventByIdAsync(int id)
        {
            return await _surveyEventRepository.GetByIdAsync(id);
        }

        public async Task<SurveyEvent> GetEventByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var events = await _surveyEventRepository.GetAllAsync(query =>
                query.Where(e => e.Code == code));

            return events.FirstOrDefault();
        }

        public bool IsEventOpen(SurveyEvent surveyEvent)
        {
            if (surveyEvent == null || !surveyEvent.IsActive)
                return false;

            var now = DateTime.UtcNow;

            if (surveyEvent.EndDateUtc.HasValue && now > surveyEvent.EndDateUtc.Value)
                return false;

            return true;
        }

        public async Task InsertEventAsync(SurveyEvent surveyEvent)
        {
            await _surveyEventRepository.InsertAsync(surveyEvent);
        }

        public async Task UpdateEventAsync(SurveyEvent surveyEvent)
        {
            await _surveyEventRepository.UpdateAsync(surveyEvent);
        }

        public async Task DeleteEventAsync(SurveyEvent surveyEvent)
        {
            await _surveyEventRepository.DeleteAsync(surveyEvent);
        }

        public async Task<string> GenerateUniqueCodeAsync(string name)
        {
            var baseCode = Slugify(name);
            if (string.IsNullOrEmpty(baseCode))
                baseCode = "event";

            var candidate = baseCode;
            var suffix = 1;

            while (await GetEventByCodeAsync(candidate) != null)
            {
                suffix++;
                candidate = $"{baseCode}-{suffix}";
            }

            return candidate;
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var lowered = value.Trim().ToLowerInvariant();
            var builder = new System.Text.StringBuilder();
            var lastWasDash = false;

            foreach (var ch in lowered)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(ch);
                    lastWasDash = false;
                }
                else if (!lastWasDash && builder.Length > 0)
                {
                    builder.Append('-');
                    lastWasDash = true;
                }
            }

            return builder.ToString().Trim('-');
        }

        public async Task<IList<SurveyCustomField>> GetCustomFieldsByEventIdAsync(int surveyEventId)
        {
            return await _surveyCustomFieldRepository.GetAllAsync(query =>
                query.Where(f => f.SurveyEventId == surveyEventId).OrderBy(f => f.DisplayOrder));
        }

        public async Task<SurveyCustomField> GetCustomFieldByIdAsync(int id)
        {
            return await _surveyCustomFieldRepository.GetByIdAsync(id);
        }

        public async Task InsertCustomFieldAsync(SurveyCustomField customField)
        {
            await _surveyCustomFieldRepository.InsertAsync(customField);
        }

        public async Task DeleteCustomFieldAsync(SurveyCustomField customField)
        {
            // The SurveyCustomField -> SurveyResponseCustomValue relationship isn't cascading
            // (SQL Server won't allow a second cascade path down from SurveyEvent - see SchemaMigration),
            // so any previously-collected answers for this field must be cleaned up explicitly first.
            var orphanedValues = await _surveyResponseCustomValueRepository.GetAllAsync(query =>
                query.Where(v => v.SurveyCustomFieldId == customField.Id));

            if (orphanedValues.Count > 0)
                await _surveyResponseCustomValueRepository.DeleteAsync(orphanedValues);

            await _surveyCustomFieldRepository.DeleteAsync(customField);
        }

        public async Task<IList<SurveyResponse>> GetResponsesByEventIdAsync(int surveyEventId)
        {
            return await _surveyResponseRepository.GetAllAsync(query =>
                query.Where(r => r.SurveyEventId == surveyEventId).OrderByDescending(r => r.CreatedOnUtc));
        }

        public async Task<SurveyResponse> GetResponseByIdAsync(int id)
        {
            return await _surveyResponseRepository.GetByIdAsync(id);
        }

        public async Task<int> GetResponseCountByEventIdAsync(int surveyEventId)
        {
            var responses = await GetResponsesByEventIdAsync(surveyEventId);
            return responses.Count;
        }

        public async Task<bool> HasResponseWithPhoneAsync(int surveyEventId, string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return await _surveyResponseRepository.Table.AnyAsync(r =>
                r.SurveyEventId == surveyEventId && r.Phone == phone);
        }

        public async Task<IList<SurveyResponseCustomValue>> GetCustomValuesByResponseIdAsync(int surveyResponseId)
        {
            return await _surveyResponseCustomValueRepository.GetAllAsync(query =>
                query.Where(v => v.SurveyResponseId == surveyResponseId));
        }

        public async Task<IDictionary<int, IList<SurveyResponseCustomValue>>> GetCustomValuesByEventIdAsync(int surveyEventId)
        {
            var responses = await GetResponsesByEventIdAsync(surveyEventId);
            var responseIds = responses.Select(r => r.Id).ToList();

            var allValues = await _surveyResponseCustomValueRepository.GetAllAsync(query =>
                query.Where(v => responseIds.Contains(v.SurveyResponseId)));

            return allValues
                .GroupBy(v => v.SurveyResponseId)
                .ToDictionary(g => g.Key, g => (IList<SurveyResponseCustomValue>)g.ToList());
        }

        public async Task InsertResponseAsync(SurveyResponse response, IDictionary<int, string> customFieldValues)
        {
            await _surveyResponseRepository.InsertAsync(response);

            if (customFieldValues == null || customFieldValues.Count == 0)
                return;

            var values = customFieldValues
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => new SurveyResponseCustomValue
                {
                    SurveyResponseId = response.Id,
                    SurveyCustomFieldId = kvp.Key,
                    Value = kvp.Value
                })
                .ToList();

            foreach (var value in values)
            {
                await _surveyResponseCustomValueRepository.InsertAsync(value);
            }
        }

        public async Task DeleteResponseAsync(SurveyResponse response)
        {
            // SurveyResponseCustomValue.SurveyResponseId cascade-deletes at the DB level
            // (see SchemaMigration) - no explicit cleanup needed here.
            await _surveyResponseRepository.DeleteAsync(response);
        }
    }
}
