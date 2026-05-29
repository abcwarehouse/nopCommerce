using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.DealTherapy.Models;
using AbcWarehouse.Plugin.Misc.DealTherapy.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Controllers
{
    public class DealTherapyController : BasePluginController
    {
        private readonly IDealTherapyService _dealTherapyService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public DealTherapyController(
            IDealTherapyService dealTherapyService,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _dealTherapyService = dealTherapyService;
            _workContext = workContext;
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new DealTherapyModel
            {
                Questions = GetQuestions()
            };

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Index.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Submit(DealTherapySubmitModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isGuest = await _customerService.IsGuestAsync(customer);
            int? customerId = !isGuest ? customer.Id : null;

            var answers = (model.Answers ?? new Dictionary<string, string>())
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();

            var submissionId = await _dealTherapyService.SaveSubmissionAsync(customerId, model.Email, answers);

            return RedirectToAction("Result", new { id = submissionId });
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var submission = await _dealTherapyService.GetSubmissionAsync(id);
            if (submission == null)
                return RedirectToAction("Index");

            var answers = await _dealTherapyService.GetAnswersForSubmissionAsync(id);
            var answerDict = answers.ToDictionary(a => a.QuestionKey, a => a.AnswerValue);

            var resultModel = new DealTherapyResultModel
            {
                Email = submission.Email,
                Answers = answerDict,
                Recommendation = BuildRecommendation(answerDict)
            };

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Result.cshtml", resultModel);
        }

        private static List<DealTherapyQuestion> GetQuestions() => new()
        {
            new DealTherapyQuestion
            {
                Key = "reason",
                Text = "What brings you here today?",
                Options = new() { "Looking for a great deal", "Treating myself", "Buying a gift", "Replacing something that broke" }
            },
            new DealTherapyQuestion
            {
                Key = "budget",
                Text = "What's your budget?",
                Options = new() { "Under $200", "$200–$500", "$500–$1,000", "$1,000+" }
            },
            new DealTherapyQuestion
            {
                Key = "category",
                Text = "What category are you most interested in?",
                Options = new() { "Appliances", "Electronics", "Furniture", "Other" }
            },
            new DealTherapyQuestion
            {
                Key = "shopping_style",
                Text = "How do you prefer to shop?",
                Options = new() { "In-store pickup", "Home delivery", "Either works for me" }
            },
            new DealTherapyQuestion
            {
                Key = "timeline",
                Text = "How soon do you need it?",
                Options = new() { "ASAP", "Within the week", "This month", "Just browsing" }
            }
        };

        private static string BuildRecommendation(Dictionary<string, string> answers)
        {
            // TODO: customize recommendation logic based on quiz answers
            var category = answers.GetValueOrDefault("category", "products");
            return $"Based on your answers, check out our {category.ToLower()} deals — we think we have something perfect for you!";
        }
    }
}
