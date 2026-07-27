using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Models;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;
using Nop.Plugin.Misc.AbcEventSurveys.Services;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.AbcEventSurveys.Areas.Admin.Controllers
{
    public class SurveyEventController : BaseAdminController
    {
        private readonly ISurveyEventService _surveyEventService;
        private readonly ISurveyExportService _surveyExportService;

        public SurveyEventController(ISurveyEventService surveyEventService, ISurveyExportService surveyExportService)
        {
            _surveyEventService = surveyEventService;
            _surveyExportService = surveyExportService;
        }

        #region Events

        public IActionResult List()
        {
            return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/List.cshtml", new SurveyEventSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> List(SurveyEventSearchModel searchModel)
        {
            var events = (await _surveyEventService.GetAllEventsAsync()).ToPagedList(searchModel);

            var model = await new SurveyEventListModel().PrepareToGridAsync(searchModel, events, () =>
                events.SelectAwait(async surveyEvent => new SurveyEventModel
                {
                    Id = surveyEvent.Id,
                    Name = surveyEvent.Name,
                    Code = surveyEvent.Code,
                    IsActive = surveyEvent.IsActive,
                    StartDateUtc = surveyEvent.StartDateUtc,
                    EndDateUtc = surveyEvent.EndDateUtc,
                    ResponseCount = await _surveyEventService.GetResponseCountByEventIdAsync(surveyEvent.Id)
                }));

            return Json(model);
        }

        public IActionResult Create()
        {
            var model = new SurveyEventModel { IsActive = true };
            return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/Create.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(SurveyEventModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/Create.cshtml", model);

            var code = !string.IsNullOrWhiteSpace(model.Code)
                ? model.Code
                : await _surveyEventService.GenerateUniqueCodeAsync(model.Name);

            var surveyEvent = new SurveyEvent
            {
                Name = model.Name,
                Code = code,
                Header1 = model.Header1,
                PictureId = model.PictureId,
                Header2 = model.Header2,
                Description = model.Description,
                IsActive = model.IsActive,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _surveyEventService.InsertEventAsync(surveyEvent);

            return RedirectToAction("Edit", new { id = surveyEvent.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(id);
            if (surveyEvent == null)
                return RedirectToAction("List");

            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(id);

            var model = new SurveyEventModel
            {
                Id = surveyEvent.Id,
                Name = surveyEvent.Name,
                Code = surveyEvent.Code,
                Header1 = surveyEvent.Header1,
                PictureId = surveyEvent.PictureId,
                Header2 = surveyEvent.Header2,
                Description = surveyEvent.Description,
                IsActive = surveyEvent.IsActive,
                StartDateUtc = surveyEvent.StartDateUtc,
                EndDateUtc = surveyEvent.EndDateUtc,
                PublicUrl = Url.RouteUrl("Plugin.Misc.AbcEventSurveys.Survey", new { code = surveyEvent.Code }, Request.Scheme),
                CustomFields = customFields.Select(f => new SurveyCustomFieldModel
                {
                    Id = f.Id,
                    SurveyEventId = f.SurveyEventId,
                    Name = f.Name,
                    IsRequired = f.IsRequired,
                    DisplayOrder = f.DisplayOrder
                }).ToList()
            };

            return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/Edit.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(SurveyEventModel model)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(model.Id);
            if (surveyEvent == null)
                return RedirectToAction("List");

            if (!ModelState.IsValid)
            {
                model.CustomFields = (await _surveyEventService.GetCustomFieldsByEventIdAsync(model.Id))
                    .Select(f => new SurveyCustomFieldModel { Id = f.Id, Name = f.Name, IsRequired = f.IsRequired, DisplayOrder = f.DisplayOrder })
                    .ToList();
                return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/Edit.cshtml", model);
            }

            surveyEvent.Name = model.Name;
            surveyEvent.Code = !string.IsNullOrWhiteSpace(model.Code) ? model.Code : surveyEvent.Code;
            surveyEvent.Header1 = model.Header1;
            surveyEvent.PictureId = model.PictureId;
            surveyEvent.Header2 = model.Header2;
            surveyEvent.Description = model.Description;
            surveyEvent.IsActive = model.IsActive;
            surveyEvent.StartDateUtc = model.StartDateUtc;
            surveyEvent.EndDateUtc = model.EndDateUtc;

            await _surveyEventService.UpdateEventAsync(surveyEvent);

            return RedirectToAction("Edit", new { id = surveyEvent.Id });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(id);
            if (surveyEvent != null)
                await _surveyEventService.DeleteEventAsync(surveyEvent);

            return RedirectToAction("List");
        }

        #endregion

        #region Custom fields

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddCustomField(int surveyEventId, string name, bool isRequired)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var existing = await _surveyEventService.GetCustomFieldsByEventIdAsync(surveyEventId);

                await _surveyEventService.InsertCustomFieldAsync(new SurveyCustomField
                {
                    SurveyEventId = surveyEventId,
                    Name = name.Trim(),
                    IsRequired = isRequired,
                    DisplayOrder = existing.Count
                });
            }

            return RedirectToAction("Edit", new { id = surveyEventId });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteCustomField(int id, int surveyEventId)
        {
            var field = await _surveyEventService.GetCustomFieldByIdAsync(id);
            if (field != null)
                await _surveyEventService.DeleteCustomFieldAsync(field);

            return RedirectToAction("Edit", new { id = surveyEventId });
        }

        #endregion

        #region Responses / export

        public async Task<IActionResult> Responses(int id)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(id);
            if (surveyEvent == null)
                return RedirectToAction("List");

            var searchModel = new SurveyResponseSearchModel
            {
                SurveyEventId = surveyEvent.Id,
                SurveyEventName = surveyEvent.Name,
                SurveyEventCode = surveyEvent.Code
            };

            return View("~/Plugins/Misc.AbcEventSurveys/Areas/Admin/Views/SurveyEvent/Responses.cshtml", searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> Responses(SurveyResponseSearchModel searchModel)
        {
            var responses = (await _surveyEventService.GetResponsesByEventIdAsync(searchModel.SurveyEventId)).ToPagedList(searchModel);
            var customFields = await _surveyEventService.GetCustomFieldsByEventIdAsync(searchModel.SurveyEventId);
            var valuesByResponseId = await _surveyEventService.GetCustomValuesByEventIdAsync(searchModel.SurveyEventId);

            var model = new SurveyResponseListModel().PrepareToGrid(searchModel, responses, () =>
                responses.Select(response =>
                {
                    valuesByResponseId.TryGetValue(response.Id, out var responseValues);

                    var additionalInfo = string.Join("; ", customFields
                        .Select(f => new
                        {
                            f.Name,
                            Value = responseValues?.FirstOrDefault(v => v.SurveyCustomFieldId == f.Id)?.Value
                        })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .Select(x => $"{x.Name}: {x.Value}"));

                    return new SurveyResponseModel
                    {
                        Id = response.Id,
                        FirstName = response.FirstName,
                        LastName = response.LastName,
                        Email = response.Email,
                        Phone = response.Phone,
                        ConsentMarketing = response.ConsentMarketing,
                        CreatedOnUtc = response.CreatedOnUtc,
                        AdditionalInfo = additionalInfo
                    };
                }));

            return Json(model);
        }

        public async Task<IActionResult> ExportXlsx(int id)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(id);
            if (surveyEvent == null)
                return RedirectToAction("List");

            var bytes = await _surveyExportService.ExportResponsesToXlsxAsync(id);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{surveyEvent.Code}-responses.xlsx");
        }

        public async Task<IActionResult> ExportXml(int id)
        {
            var surveyEvent = await _surveyEventService.GetEventByIdAsync(id);
            if (surveyEvent == null)
                return RedirectToAction("List");

            var bytes = await _surveyExportService.ExportResponsesToXmlAsync(id);
            return File(bytes, "application/xml", $"{surveyEvent.Code}-responses.xml");
        }

        #endregion
    }
}
