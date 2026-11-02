using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Models
{
    public partial record SurveyResponseModel : BaseNopEntityModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool ConsentMarketing { get; set; }
        public bool ConsentSms { get; set; }
        /// <summary>
        /// Submission time already converted to Detroit time and formatted server-side, so the
        /// grid doesn't re-convert it to the viewer's browser time zone.
        /// </summary>
        public string SubmittedOn { get; set; }

        /// <summary>
        /// Traffic attribution flattened as "source / medium".
        /// </summary>
        public string TrafficSource { get; set; }

        /// <summary>
        /// All custom field answers for this response, flattened as "Field: value; Field2: value2".
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}
