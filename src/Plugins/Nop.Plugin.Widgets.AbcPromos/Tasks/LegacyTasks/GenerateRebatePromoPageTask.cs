using Nop.Services.Tasks;
using Nop.Plugin.Misc.AbcCore.Extensions;
using Nop.Core.Domain.Topics;
using Nop.Services.Topics;
using Nop.Services.Seo;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using System.Linq;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Services.Catalog;
using System.Text;
using Nop.Plugin.Misc.AbcCore.Services;
using Nop.Core.Domain.Media;
using Nop.Services.Media;
using Nop.Services.Logging;
using Nop.Plugin.Widgets.AbcPromos;
using Task = System.Threading.Tasks.Task;
using Nop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Widgets.AbcPromos.Tasks.LegacyTasks
{
    public class GenerateRebatePromoPageTask : IScheduleTask
    {
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductAbcDescription> _productAbcDescriptionRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

        private readonly IAbcPromoService _abcPromoService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;
       

       

        private readonly MediaSettings _mediaSettings;
        private readonly AbcPromosSettings _settings;

        public GenerateRebatePromoPageTask(
            IRepository<Topic> topicRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAbcDescription> productAbcDescriptionRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IAbcPromoService abcPromoService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IProductService productService,
            ITopicService topicService,
            IUrlRecordService urlRecordService,
            
            MediaSettings mediaSettings,
            AbcPromosSettings settings
            
        )
        {
            _topicRepository = topicRepository;
            _productRepository = productRepository;
            _productAbcDescriptionRepository = productAbcDescriptionRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _abcPromoService = abcPromoService;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _productService = productService;
            _topicService = topicService;
            _urlRecordService = urlRecordService;
            
            _mediaSettings = mediaSettings;
            _settings = settings;
           
        }

        public async Task ExecuteAsync()
        {
            var rebatePromoTopicName = "Rebates and Promos";
            var topic = await _topicService.GetTopicBySystemNameAsync(rebatePromoTopicName);
            if (topic == null)
            {
                topic = new Topic
                {
                    SystemName = rebatePromoTopicName,
                    IncludeInFooterColumn1 = true,
                    LimitedToStores = false,
                    Title = rebatePromoTopicName,
                    TopicTemplateId = 1,
                    Published = true
                };
                await _topicService.InsertTopicAsync(topic);
                await _urlRecordService.SaveSlugAsync(topic, await _urlRecordService.ValidateSeNameAsync(topic, rebatePromoTopicName, topic.Title, true), 0);
            }

            topic.Body = await GetRebatePromoHtmlAsync(topic);
            await _topicService.UpdateTopicAsync(topic);
        }

        private async System.Threading.Tasks.Task<string> GetRebatePromoHtmlAsync(Topic rootTopic)
{
    var html = $"<h2 class=\"abc-rebate-promo-title\">" +
               "Promos</h2><div class=\"abc-container abc-promo-container\">";

    var promos = _settings.IncludeExpiredPromosOnRebatesPromosPage ?
                 (await _abcPromoService.GetActivePromosAsync()).Union(await _abcPromoService.GetExpiredPromosAsync()) :
                 await _abcPromoService.GetActivePromosAsync();

    // Create a list to hold promo data before sorting
    var promoList = new List<(string manName, string promoHtml)>();

    foreach (var promo in promos)
    {
        var publishedPromoProducts = await _abcPromoService.GetPublishedProductsByPromoIdAsync(promo.Id);
        if (!publishedPromoProducts.Any())
        {
            continue;
        }

        // Handle manufacturer retrieval safely
        var manufacturerId = promo.ManufacturerId ?? 0;
        var manufactureModel = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
        if (manufactureModel == null)
        {
            continue; // Skip this promo if manufacturer is null
        }

        string manName = string.IsNullOrEmpty(manufactureModel.Name) ? "Universal" : manufactureModel.Name;

        var promoDescription = $"{manName} - {promo.Description}";

        string promoHtml = $"<div class=\"abc-item abc-promo-item\"> " +
                           $"<h1>{manName}</h1>" +
                           $"<a href=\"/promos/{await _urlRecordService.GetActiveSlugAsync(promo.Id, "AbcPromo", 0) ?? "default-slug"}\"> " +
                           $"{promoDescription}</a><br />" +
                           $"Expires {promo.EndDate.ToString("MM-dd-yy")}" +
                           "</div>";

        // Add promo to list for sorting
        promoList.Add((manName, promoHtml));
    }

    // Sort promos alphabetically by manufacturer name
    foreach (var (manName, promoHtml) in promoList.OrderBy(p => p.manName))
    {
        html += promoHtml;
        html += $"<a class=\"ManButton\" href=\"{manName}\">Shop {manName}</a>";
    }

    html += "</div>";
    return html;
}
    }
}