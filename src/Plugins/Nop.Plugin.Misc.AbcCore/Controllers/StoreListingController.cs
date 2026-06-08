using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Topics;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.AbcCore.Controllers
{
    public class StoreListingController : BasePluginController
    {
        private readonly ILogger _logger;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;

        public StoreListingController(
            ILogger logger,
            IAclService aclService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ITopicModelFactory topicModelFactory,
            ITopicService topicService,
            IUrlRecordService urlRecordService
        ) {
            _logger = logger;
            _aclService = aclService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _topicModelFactory = topicModelFactory;
            _topicService = topicService;
            _urlRecordService = urlRecordService;
        }

        public async Task<IActionResult> Details(string storeSlug)
        {
            await _logger.InformationAsync($"Store details requested for slug: {storeSlug}");

            if (string.IsNullOrWhiteSpace(storeSlug))
                return NotFound();

            var topic = default(Topic);

            // First try resolving by SEO slug if it points to a topic.
            var urlRecord = await _urlRecordService.GetBySlugAsync(storeSlug);
            if (urlRecord is not null && urlRecord.EntityName.Equals(nameof(Topic), StringComparison.InvariantCultureIgnoreCase))
                topic = await _topicService.GetTopicByIdAsync(urlRecord.EntityId);

            // Fallback: allow direct topic system name usage in storeSlug.
            topic ??= await _topicService.GetTopicBySystemNameAsync(storeSlug);

            if (topic is null)
                return NotFound();

            var notAvailable = !topic.Published ||
                               !await _aclService.AuthorizeAsync(topic) ||
                               !await _storeMappingService.AuthorizeAsync(topic);

            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL)
                                 && await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_VIEW);

            if (notAvailable && !hasAdminAccess)
                return NotFound();

            var model = await _topicModelFactory.PrepareTopicModelAsync(topic);

            if (hasAdminAccess)
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = AreaNames.ADMIN }));

            return View("~/Plugins/Misc.AbcFrontend/Views/Topic/TopicDetails.cshtml", model);
        }
    }
}
