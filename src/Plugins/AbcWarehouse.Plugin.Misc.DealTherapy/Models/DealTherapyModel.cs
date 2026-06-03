using System.Collections.Generic;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Models
{
    public class QuizOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string RevealBranch { get; set; }
        public string RevealSubBranch { get; set; }
    }

    public class DealTherapyQuestion
    {
        public string Key { get; set; }
        public string IntroText { get; set; }
        public string Text { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ImageUrl { get; set; }
        public List<QuizOption> Options { get; set; }
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

    public class ProductResult
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Diagnosis { get; set; }
        public string Verdict { get; set; }
        public string SideEffects { get; set; }
        public string Sku { get; set; }
        public string CustomImageUrl { get; set; }
    }

    public class DealTherapyProductPreviewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Price { get; set; }
        public string ProductUrl { get; set; }
    }

    public class DealTherapyResultModel
    {
        public string Email { get; set; }
        public ProductResult Product { get; set; }
        public DealTherapyProductPreviewModel ProductPreview { get; set; }
    }

    public class DealTherapySubmissionRowModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Email { get; set; }
        public string CreatedOn { get; set; }
        public string ProductName { get; set; }
        public string Diagnosis { get; set; }
        public string Branch { get; set; }
    }
}
