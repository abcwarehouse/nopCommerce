using Nop.Core;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Domain
{
    public class DealTherapyAnswer : BaseEntity
    {
        public int SubmissionId { get; set; }
        public string QuestionKey { get; set; }
        public string AnswerValue { get; set; }
    }
}
