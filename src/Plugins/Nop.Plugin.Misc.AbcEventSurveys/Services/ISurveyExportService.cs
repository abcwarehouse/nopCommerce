namespace Nop.Plugin.Misc.AbcEventSurveys.Services
{
    public interface ISurveyExportService
    {
        /// <summary>
        /// Exports all responses for the given event to an .xlsx workbook.
        /// </summary>
        Task<byte[]> ExportResponsesToXlsxAsync(int surveyEventId);

        /// <summary>
        /// Exports all responses for the given event to an .xml document.
        /// </summary>
        Task<byte[]> ExportResponsesToXmlAsync(int surveyEventId);
    }
}
