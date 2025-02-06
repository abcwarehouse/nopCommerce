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
            var html = $"<h2 class=\"abc-rebate-promo-title\"></h2><div class=\"abc-container abc-promo-container\">";

            var promos = _settings.IncludeExpiredPromosOnRebatesPromosPage ?
                         (await _abcPromoService.GetActivePromosAsync()).Union(await _abcPromoService.GetExpiredPromosAsync()) :
                         await _abcPromoService.GetActivePromosAsync();

            // Dictionary to store manufacturer names and their promos
            var promoGroups = new Dictionary<string, List<AbcPromo>>();

            foreach (var promo in promos)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(promo.ManufacturerId ?? 0);
                var manName = manufacturer?.Name ?? "Universal";

                if (!promoGroups.ContainsKey(manName))
                {
                    promoGroups[manName] = new List<AbcPromo>();
                }

                promoGroups[manName].Add(promo);
            }

            foreach (var group in promoGroups.OrderBy(g => g.Key))
            {
                string manName = group.Key;



                html += $"<h1>{manName}</h1>";

                foreach (var promo in group.Value)
                {
                    string promoSlug = await _urlRecordService.GetActiveSlugAsync(promo.Id, "AbcPromo", 0) ?? "default-slug";
                    var promoDescription = $"{manName} - {promo.Description}";

                    html += $"<div class=\"abc-item abc-promo-item\"> " +
                           $"<a class=\"promo-link\" href=\"/promos/{promoSlug}\">{promoDescription}</a>" +
                           $" - Expires {promo.EndDate:MM-dd-yy}<br />" +
                           "</div>";
                }

                if (manName.ToLower() == "profile")
                {
                    html += $"<a class=\"ManButton\" href=\"/profile-2\">Shop {manName}</a>";
                }
                else if (manName.ToLower() == "deal partners")
                {
                    html += $"<a class=\"ManButton\" href=\"/deal-partners-llc\">Shop {manName}</a>";
                }
                 else if (manName.ToLower() == "universal")
                {
                    html += $"<a class=\"ManButton\" href=\"/manufacturer/all\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "atg....audio to go")

                {
                    html += $"<a class=\"ManButton\" href=\"/atgaudio-to-go\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "black and decker")
                {
                    html += $"<a class=\"ManButton\" href=\"/black-decker\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "fisher & paykel")
                {
                    html += $"<a class=\"ManButton\" href=\"/fisher-paykel\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "g.e. cafe series")
                {
                    html += $"<a class=\"ManButton\" href=\"/ge-cafe-series\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "leggett & platt")
                {
                    html += $"<a class=\"ManButton\" href=\"/leggett-platt\">Shop {manName}</a>";
                }


                 else if (manName.ToLower() == "nectar")
                {
                    html += $"<a class=\"ManButton\" href=\"/nectar-3\">Shop {manName}</a>";
                }


                 else if (manName.ToLower() == "panasonic energy c/o amer")
                {
                    html += $"<a class=\"ManButton\" href=\"/panasonic-energy-co-amer\">Shop {manName}</a>";
                }

                  else if (manName.ToLower() == "roku streaming stick+")
                {
                    html += $"<a class=\"ManButton\" href=\"/roku-streaming-stick-plus\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "sealy")
                {
                    html += $"<a class=\"ManButton\" href=\"/sealy-4\">Shop {manName}</a>";
                }

                else if (manName.ToLower() == "sunbrite tv")
                {
                    html += $"<a class=\"ManButton\" href=\"/sunbritetv\">Shop {manName}</a>";
                }

                else if (manName.ToLower() == "sunbrite tv")
                {
                    html += $"<a class=\"ManButton\" href=\"/sunbritetv\">Shop {manName}</a>";
                }

                else if (manName.ToLower() == "tempur-pedic")
                {
                    html += $"<a class=\"ManButton\" href=\"/tempur-pedic-mattress\">Shop {manName}</a>";
                }

                 else if (manName.ToLower() == "tempur-pedic")
                {
                    html += $"<a class=\"ManButton\" href=\"/tempur-pedic-mattress\">Shop {manName}</a>";
                }

                else
                {
                    string formattedManName = manName.Replace(" ", "-")
                                                     .Replace("/", "")
                                                     .Replace(".", "")
                                                     .Replace(",", "");
                    html += $"<a class=\"ManButton\" href=\"/{formattedManName}\">Shop {manName}</a>";
                }

            }

            return html;
        }
    }
}