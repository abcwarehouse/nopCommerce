using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Topics;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.AbcCore.Controllers
{
    public class StoreListingController : BasePublicController
    {
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;

        public StoreListingController(
            IAclService aclService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ITopicModelFactory topicModelFactory,
            ITopicService topicService,
            IUrlRecordService urlRecordService
        )
        {
            _aclService = aclService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _topicModelFactory = topicModelFactory;
            _topicService = topicService;
            _urlRecordService = urlRecordService;
        }

        public async Task<IActionResult> Details(string storeSlug)
        {
            if (string.IsNullOrWhiteSpace(storeSlug))
                return InvokeHttp404();

            var topic = default(Topic);

            // First try resolving by SEO slug if it points to a topic.
            var urlRecord = await _urlRecordService.GetBySlugAsync(storeSlug);
            if (urlRecord is not null && urlRecord.EntityName.Equals(nameof(Topic), StringComparison.InvariantCultureIgnoreCase))
            {
                if (!urlRecord.IsActive)
                {
                    var activeSlug = await _urlRecordService.GetActiveSlugAsync(urlRecord.EntityId, urlRecord.EntityName, urlRecord.LanguageId);
                    if (!string.IsNullOrEmpty(activeSlug))
                        return RedirectToRoutePermanent("StoreListingDetails", new { storeSlug = activeSlug });

                    return InvokeHttp404();
                }

                topic = await _topicService.GetTopicByIdAsync(urlRecord.EntityId);
            }

            // Fallback: allow direct topic system name usage in storeSlug.
            topic ??= await _topicService.GetTopicBySystemNameAsync(storeSlug);

            if (topic is null)
                return InvokeHttp404();

            var notAvailable = !topic.Published ||
                               !await _aclService.AuthorizeAsync(topic) ||
                               !await _storeMappingService.AuthorizeAsync(topic);

            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL)
                                 && await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_VIEW);

            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            var model = await _topicModelFactory.PrepareTopicModelAsync(topic);

            if (hasAdminAccess)
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = AreaNames.ADMIN }));

            return View("~/Plugins/Misc.AbcFrontend/Views/Topic/TopicDetails.cshtml", model);
        }
    }
}
