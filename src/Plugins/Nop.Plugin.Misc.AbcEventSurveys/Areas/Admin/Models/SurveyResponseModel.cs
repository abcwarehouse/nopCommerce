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
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// All custom field answers for this response, flattened as "Field: value; Field2: value2".
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}
