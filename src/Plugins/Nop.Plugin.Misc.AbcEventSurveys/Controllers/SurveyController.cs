using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;
using Nop.Plugin.Misc.AbcEventSurveys.Models;
using Nop.Plugin.Misc.AbcEventSurveys.Services;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AbcEventSurveys.Controllers
{
    public class SurveyController : BasePluginController
    {
        private readonly ISurveyEventService _surveyEventService;
        private readonly IListrakService _listrakService;
        private readonly ILogger _logger;

        public SurveyController(ISurveyEventService surveyEventService, IListrakService listrakService, ILogger logger)
        {
            _surveyEventService = surveyEventService;
            _listrakService = listrakService;
            _logger = logger;
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

            if (!postedModel.ConsentMarketing)
            {
                ModelState.AddModelError(nameof(SurveyPageModel.ConsentMarketing),
                    "You must agree to the Terms and Conditions to enter.");
            }

            if (!ModelState.IsValid)
            {
                postedModel.SurveyEventId = surveyEvent.Id;
                postedModel.Code = surveyEvent.Code;
                postedModel.Header1 = surveyEvent.Header1;
                postedModel.PictureId = surveyEvent.PictureId;
                postedModel.Header2 = surveyEvent.Header2;
                postedModel.Description = surveyEvent.Description;
                postedModel.TermsAndConditions = surveyEvent.TermsAndConditions;
                postedModel.EndDateUtc = surveyEvent.EndDateUtc;
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
                Phone = CleanPhoneNumber(postedModel.Phone),
                ConsentMarketing = postedModel.ConsentMarketing,
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var customValues = customFields
                .Select((field, i) => new { field.Id, Value = i < postedModel.CustomFields.Count ? postedModel.CustomFields[i].Value : null })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Id, x => x.Value);

            await _surveyEventService.InsertResponseAsync(response, customValues);

            if (response.ConsentMarketing && !string.IsNullOrWhiteSpace(response.Phone))
            {
                await TrySubscribeToListrakSmsAsync(response.Phone, response.FirstName, response.LastName, response.Email);
            }

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
            model.PictureId = surveyEvent.PictureId;
            model.Header2 = surveyEvent.Header2;
            model.Description = surveyEvent.Description;
            model.TermsAndConditions = surveyEvent.TermsAndConditions;
            model.ThankYouHeader = surveyEvent.ThankYouHeader;
            model.ThankYouDescription = surveyEvent.ThankYouDescription;
            model.EndDateUtc = surveyEvent.EndDateUtc;

            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEvent.Id);
            model.CustomFields = customFields.Select(field => new SurveyCustomFieldInputModel
            {
                Id = field.Id,
                Name = field.Name,
                IsRequired = field.IsRequired
            }).ToList();

            return model;
        }

        /// <summary>
        /// Strips formatting characters entered in the phone field so only digits (and a
        /// possible leading "+") are stored, e.g. "(586) 123-4567" -> "5861234567".
        /// </summary>
        private static string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            return Regex.Replace(phone, @"[-()\s]", string.Empty);
        }

        /// <summary>
        /// Adds the entrant to Listrak's SMS phone list. This is best-effort: entering the
        /// survey must always succeed even if Listrak is unreachable or the number is already
        /// on the list (which Listrak reports as an HTTP 400 - that's an expected outcome here,
        /// not a signup failure, so it's only logged, never surfaced to the entrant).
        /// </summary>
        private async Task TrySubscribeToListrakSmsAsync(string phone, string firstName, string lastName, string email)
        {
            var digitsOnly = Regex.Replace(phone ?? string.Empty, @"\D", string.Empty);

            // Listrak expects a bare 10-digit number; drop a leading US country code if present.
            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("1"))
            {
                digitsOnly = digitsOnly[1..];
            }

            if (digitsOnly.Length != 10)
            {
                await _logger.InformationAsync($"Widgets.Listrak: skipped SMS subscribe for event survey entrant - '{phone}' isn't a 10-digit number.");
                return;
            }

            try
            {
                var listrakResponse = await _listrakService.SubscribeOrEnrichPhoneNumberAsync(digitsOnly, firstName, lastName, email);

                if (!listrakResponse.IsSuccessStatusCode)
                {
                    var content = await listrakResponse.Content.ReadAsStringAsync();
                    await _logger.InformationAsync($"Widgets.Listrak: SMS subscribe returned {(int)listrakResponse.StatusCode} for event survey entrant (e.g. already subscribed) - {content}");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Widgets.Listrak: SMS subscribe failed for event survey entrant.", ex);
            }
        }
    }
}
