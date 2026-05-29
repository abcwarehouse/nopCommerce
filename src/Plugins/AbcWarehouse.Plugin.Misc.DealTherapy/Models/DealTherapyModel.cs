using System.Collections.Generic;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Models
{
    public class DealTherapyQuestion
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
    }

    public class DealTherapyModel
    {
        public string Email { get; set; }
        public List<DealTherapyQuestion> Questions { get; set; }
    }

    public class DealTherapySubmitModel
    {
        public string Email { get; set; }
        public Dictionary<string, string> Answers { get; set; } = new();
    }

    public class DealTherapyResultModel
    {
        public string Email { get; set; }
        public Dictionary<string, string> Answers { get; set; }
        public string Recommendation { get; set; }
    }
}
