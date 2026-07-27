using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Models
{
    public partial record SurveyCustomFieldModel : BaseNopEntityModel
    {
        public int SurveyEventId { get; set; }

        [Required]
        [Display(Name = "Field Label")]
        public string Name { get; set; }

        [Display(Name = "Required")]
        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }
    }
}
