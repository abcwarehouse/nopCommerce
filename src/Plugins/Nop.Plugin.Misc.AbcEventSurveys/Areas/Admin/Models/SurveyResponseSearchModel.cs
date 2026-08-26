using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Models
{
    public partial record SurveyResponseSearchModel : BaseSearchModel
    {
        public int SurveyEventId { get; set; }
        public string SurveyEventName { get; set; }
        public string SurveyEventCode { get; set; }
    }
}
