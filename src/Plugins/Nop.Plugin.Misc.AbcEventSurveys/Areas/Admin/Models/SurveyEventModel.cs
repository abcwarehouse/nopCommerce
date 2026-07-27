using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Models
{
    public partial record SurveyEventModel : BaseNopEntityModel
    {
        public SurveyEventModel()
        {
            CustomFields = new List<SurveyCustomFieldModel>();
        }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Code (URL identifier)")]
        public string Code { get; set; }

        [Display(Name = "Header 1")]
        public string Header1 { get; set; }

        [Display(Name = "Picture (shown below Header 1)")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [Display(Name = "Header 2")]
        public string Header2 { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Start Date")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDateUtc { get; set; }

        [Display(Name = "End Date")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDateUtc { get; set; }

        public int ResponseCount { get; set; }

        public string PublicUrl { get; set; }

        public IList<SurveyCustomFieldModel> CustomFields { get; set; }
    }
}
