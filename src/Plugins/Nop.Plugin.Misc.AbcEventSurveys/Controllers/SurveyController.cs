using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;
using Nop.Plugin.Misc.AbcEventSurveys.Models;
using Nop.Plugin.Misc.AbcEventSurveys.Services;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AbcEventSurveys.Controllers
{
    public class SurveyController : BasePluginController
    {
        private readonly ISurveyEventService _surveyEventService;

        public SurveyController(ISurveyEventService surveyEventService)
        {
            _surveyEventService = surveyEventService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string code)
        {
            var surveyEvent = await _surveyEventService.GetEventByCodeAsync(code);
            var model = await BuildPageModelAsync(surveyEvent);

            return View("~/Plugins/Misc.AbcEventSurveys/Views/Survey/Index.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Index(string code, SurveyPageModel postedModel)
        {
            var surveyEvent = await _surveyEventService.GetEventByCodeAsync(code);

            if (surveyEvent == null || !_surveyEventService.IsEventOpen(surveyEvent))
            {
                var closedModel = await BuildPageModelAsync(surveyEvent);
                return View("~/Plugins/Misc.AbcEventSurveys/Views/Survey/Index.cshtml", closedModel);
            }

            // re-attach the custom field definitions (name/required) - only the entered values came back from the form
            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEvent.Id);
            for (var i = 0; i < customFields.Count && i < postedModel.CustomFields.Count; i++)
            {
                if (customFields[i].IsRequired && string.IsNullOrWhiteSpace(postedModel.CustomFields[i].Value))
                {
                    ModelState.AddModelError($"{nameof(SurveyPageModel.CustomFields)}[{i}].{nameof(SurveyCustomFieldInputModel.Value)}",
                        $"{customFields[i].Name} is required.");
                }
            }

            if (!ModelState.IsValid)
            {
                postedModel.SurveyEventId = surveyEvent.Id;
                postedModel.Code = surveyEvent.Code;
                postedModel.Header1 = surveyEvent.Header1;
                postedModel.Header2 = surveyEvent.Header2;
                postedModel.Description = surveyEvent.Description;
                postedModel.CustomFields = customFields.Select((field, i) => new SurveyCustomFieldInputModel
                {
                    Id = field.Id,
                    Name = field.Name,
                    IsRequired = field.IsRequired,
                    Value = i < postedModel.CustomFields.Count ? postedModel.CustomFields[i].Value : null
                }).ToList();

                return View("~/Plugins/Misc.AbcEventSurveys/Views/Survey/Index.cshtml", postedModel);
            }

            var response = new SurveyResponse
            {
                SurveyEventId = surveyEvent.Id,
                FirstName = postedModel.FirstName,
                LastName = postedModel.LastName,
                Email = postedModel.Email,
                Phone = postedModel.Phone,
                ConsentMarketing = postedModel.ConsentMarketing,
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var customValues = customFields
                .Select((field, i) => new { field.Id, Value = i < postedModel.CustomFields.Count ? postedModel.CustomFields[i].Value : null })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Id, x => x.Value);

            await _surveyEventService.InsertResponseAsync(response, customValues);

            var thankYouModel = await BuildPageModelAsync(surveyEvent);
            thankYouModel.Submitted = true;

            return View("~/Plugins/Misc.AbcEventSurveys/Views/Survey/Index.cshtml", thankYouModel);
        }

        private async Task<SurveyPageModel> BuildPageModelAsync(SurveyEvent surveyEvent)
        {
            var model = new SurveyPageModel
            {
                EventClosed = surveyEvent == null || !_surveyEventService.IsEventOpen(surveyEvent)
            };

            if (surveyEvent == null)
                return model;

            model.SurveyEventId = surveyEvent.Id;
            model.Code = surveyEvent.Code;
            model.Header1 = surveyEvent.Header1;
            model.Header2 = surveyEvent.Header2;
            model.Description = surveyEvent.Description;

            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEvent.Id);
            model.CustomFields = customFields.Select(field => new SurveyCustomFieldInputModel
            {
                Id = field.Id,
                Name = field.Name,
                IsRequired = field.IsRequired
            }).ToList();

            return model;
        }
    }
}
