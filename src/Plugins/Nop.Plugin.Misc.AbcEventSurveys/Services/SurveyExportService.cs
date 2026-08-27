using System.Xml.Linq;
using ClosedXML.Excel;

namespace Nop.Plugin.Misc.AbcEventSurveys.Services
{
    public class SurveyExportService : ISurveyExportService
    {
        private readonly ISurveyEventService _surveyEventService;

        public SurveyExportService(ISurveyEventService surveyEventService)
        {
            _surveyEventService = surveyEventService;
        }

        public async Task<byte[]> ExportResponsesToXlsxAsync(int surveyEventId)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(surveyEventId);
            var responses = await _surveyEventService.GetResponsesByEventIdAsync(surveyEventId);
            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEventId);
            var valuesByResponseId = await _surveyEventService.GetCustomValuesByEventIdAsync(surveyEventId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(surveyEvent?.Code ?? "Responses");

            var col = 1;
            worksheet.Cell(1, col++).Value = "Event Code";
            worksheet.Cell(1, col++).Value = "First Name";
            worksheet.Cell(1, col++).Value = "Last Name";
            worksheet.Cell(1, col++).Value = "Email";
            worksheet.Cell(1, col++).Value = "Phone";
            worksheet.Cell(1, col++).Value = "Marketing Consent";
            worksheet.Cell(1, col++).Value = "Submitted (UTC)";

            var customFieldStartCol = col;
            foreach (var field in customFields)
            {
                worksheet.Cell(1, col++).Value = field.Name;
            }

            var row = 2;
            foreach (var response in responses)
            {
                col = 1;
                worksheet.Cell(row, col++).Value = surveyEvent?.Code;
                worksheet.Cell(row, col++).Value = response.FirstName;
                worksheet.Cell(row, col++).Value = response.LastName;
                worksheet.Cell(row, col++).Value = response.Email;
                worksheet.Cell(row, col++).Value = response.Phone;
                worksheet.Cell(row, col++).Value = response.ConsentMarketing ? "Yes" : "No";
                worksheet.Cell(row, col++).Value = response.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss");

                valuesByResponseId.TryGetValue(response.Id, out var responseValues);

                var fieldCol = customFieldStartCol;
                foreach (var field in customFields)
                {
                    var value = responseValues?.FirstOrDefault(v => v.SurveyCustomFieldId == field.Id)?.Value;
                    worksheet.Cell(row, fieldCol++).Value = value ?? string.Empty;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportResponsesToXmlAsync(int surveyEventId)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(surveyEventId);
            var responses = await _surveyEventService.GetResponsesByEventIdAsync(surveyEventId);
            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEventId);
            var valuesByResponseId = await _surveyEventService.GetCustomValuesByEventIdAsync(surveyEventId);

            var responseElements = responses.Select(response =>
            {
                valuesByResponseId.TryGetValue(response.Id, out var responseValues);

                var customFieldElements = customFields.Select(field =>
                {
                    var value = responseValues?.FirstOrDefault(v => v.SurveyCustomFieldId == field.Id)?.Value;
                    return new XElement("CustomField",
                        new XAttribute("name", field.Name),
                        value ?? string.Empty);
                });

                return new XElement("Response",
                    new XElement("FirstName", response.FirstName),
                    new XElement("LastName", response.LastName),
                    new XElement("Email", response.Email),
                    new XElement("Phone", response.Phone),
                    new XElement("MarketingConsent", response.ConsentMarketing),
                    new XElement("SubmittedOnUtc", response.CreatedOnUtc.ToString("O")),
                    new XElement("CustomFields", customFieldElements));
            });

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("SurveyEventExport",
                    new XAttribute("eventCode", surveyEvent?.Code ?? string.Empty),
                    new XAttribute("eventName", surveyEvent?.Name ?? string.Empty),
                    new XElement("Responses", responseElements)));

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }
    }
}
