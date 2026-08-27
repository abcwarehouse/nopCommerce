using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbcEventSurveys.Models
{
    /// <summary>
    /// One admin-defined custom field rendered on the public survey page, along with
    /// whatever value the entrant typed in.
    /// </summary>
    public record SurveyCustomFieldInputModel : BaseNopModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }
    }
}
