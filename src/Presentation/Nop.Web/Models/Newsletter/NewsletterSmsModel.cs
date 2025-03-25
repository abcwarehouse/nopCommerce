using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Newsletter
{
    public partial record NewsletterSmsModel : BaseNopModel
    {
        [DataType(DataType.EmailAddress)]
        public string NewsletterSmsEmail { get; set; }
        public string PhoneNumber { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}