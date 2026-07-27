using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Models
{
    public record SurveyPageModel : BaseNopModel
    {
        public SurveyPageModel()
        {
            CustomFields = new List<SurveyCustomFieldInputModel>();
        }

        public int SurveyEventId { get; set; }
        public string Code { get; set; }

        public string Header1 { get; set; }
        public int PictureId { get; set; }
        public string Header2 { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(400)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(400)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        // Deliberately no [Range(typeof(bool), ...)] here: jQuery's unobtrusive "range" validator
        // treats the value as a number (parseFloat("true") => NaN), so it fails client-side
        // regardless of whether the box is checked. Enforced manually server-side instead
        // (see SurveyController.Index POST).
        public bool ConsentMarketing { get; set; }

        public IList<SurveyCustomFieldInputModel> CustomFields { get; set; }

        /// <summary>
        /// Set after a successful submission so the view can swap the form for a thank-you message.
        /// </summary>
        public bool Submitted { get; set; }

        /// <summary>
        /// True if the event isn't found, isn't active, or is outside its date window.
        /// </summary>
        public bool EventClosed { get; set; }
    }
}
