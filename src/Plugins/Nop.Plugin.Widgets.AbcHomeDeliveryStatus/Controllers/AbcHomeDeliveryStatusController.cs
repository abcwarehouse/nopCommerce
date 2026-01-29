using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbcCore;
using Nop.Plugin.Widgets.AbcHomeDeliveryStatus.Models;
using Nop.Web.Framework.Controllers;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Nop.Plugin.Widgets.AbcHomeDeliveryStatus.Controllers
{
    public class AbcHomeDeliveryStatusController : BasePluginController
    {
        private readonly AbcHomeDeliveryStatusSettings _settings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        public AbcHomeDeliveryStatusController(
            AbcHomeDeliveryStatusSettings settings,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _settings = settings;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public IActionResult Configure()
        {
            return View("~/Plugins/Widgets.AbcHomeDeliveryStatus/Views/ConfigureHomeDeliveryStatus.cshtml", _settings.ToModel());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(AbcHomeDeliveryStatusConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var settings = AbcHomeDeliveryStatusSettings.FromModel(model);
            await _settingService.SaveSettingAsync(settings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult DisplayHomeDeliveryStatus(string invoice, string zipcode)
        {
            HomeDeliveryStatusModel model = new HomeDeliveryStatusModel();
            string xmlRequestString = BuildXmlRequestString(invoice, zipcode);
            model.Invoice = invoice;
            model.Zipcode = zipcode;

            // Check if we should use mock responses
            if (_settings.UseMockResponses)
            {
                model.StatusInfo = GetMockStatusInfo(invoice, zipcode);
                return PartialView("~/Plugins/Widgets.AbcHomeDeliveryStatus/Views/_HomeDeliveryResults.cshtml", model);
            }

            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(xmlRequestString, Encoding.UTF8, "text/xml");
                try
                {
                    var response = client.PostAsync(AbcConstants.StatusAPIUrl, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string xmlResponse = response.Content.ReadAsStringAsync().Result;
                        XmlDocument xml = new XmlDocument();
                        // parse response into status information
                        xml.LoadXml(xmlResponse);
                        var topNodes = xml.SelectNodes("Response");
                        foreach (XmlNode node in topNodes)
                        {
                            xmlResponse = node.InnerXml.ToString();
                        }

                        byte[] byteArray = Encoding.UTF8.GetBytes(xmlResponse);
                        StreamReader reader = new StreamReader(new MemoryStream(byteArray));
                        XmlSerializer serializer = new XmlSerializer(typeof(StatusInfo));
                        StatusInfo statusInfo = (StatusInfo)serializer.Deserialize(reader);
                        model.StatusInfo = statusInfo;
                        if (model.StatusInfo.InvoiceNumber.Trim().ToLower() == "not found!")
                        {
                            model.StatusInfo.ErrorMessage = "Status information not found. Please re-enter information";
                        }
                    }
                }
                catch (Exception exception)
                {
                    string message = "Error occurred, please reload the page and try again.Error: " + exception.Message;
                    if (exception.InnerException != null)
                    {
                        message += " " + exception.InnerException.Message;
                    }
                    // return an error view
                    StatusInfo errorInfo = new StatusInfo
                    {
                        ErrorMessage = message
                    };
                    model.StatusInfo = errorInfo;
                }
            }

            return PartialView("~/Plugins/Widgets.AbcHomeDeliveryStatus/Views/_HomeDeliveryResults.cshtml", model);
        }

        private string BuildXmlRequestString(string invoice, string zipcode)
        {
            XElement xml = new XElement("Request",
                new XElement("Delivery_Lookup",
                    new XElement("INVOICE", invoice),
                    new XElement("ZIPCODE", zipcode)
                )
            );
            return xml.ToString();
        }

        // needs to be updated to a more accurate response
        private StatusInfo GetMockStatusInfo(string invoice, string zipcode)
        {
            return new StatusInfo
            {
                InvoiceNumber = invoice,
                CustomerName = "John Doe",
                ShippingAddress = "123 Main St, Ann Arbor, MI " + zipcode,
                TruckLoaded = "Yes",
                DeliveryTime = "Between 10:00 AM and 2:00 PM",
                StopNumber = "3",
                StorePhoneNumber = "(734) 555-0123",
                ItemStatuses = new ItemStatus[]
                {
                    new ItemStatus
                    {
                        Model = "UN65TU8000",
                        Brand = "Samsung",
                        Name = "65\" 4K Smart TV",
                        ScheduledDeliveryDate = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy"),
                        Comment = "Out for Delivery"
                    },
                    new ItemStatus
                    {
                        Model = "LFXS26973S",
                        Brand = "LG",
                        Name = "French Door Refrigerator",
                        ScheduledDeliveryDate = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy"),
                        Comment = "Out for Delivery"
                    }
                }
            };
        }
    }
}
