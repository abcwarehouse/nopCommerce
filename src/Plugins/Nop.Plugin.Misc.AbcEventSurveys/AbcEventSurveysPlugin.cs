using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.AbcEventSurveys
{
    public class AbcEventSurveysPlugin : BasePlugin, IMiscPlugin, IConsumer<AdminMenuCreatedEvent>
    {
        private readonly IWebHelper _webHelper;

        public AbcEventSurveysPlugin(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/SurveyEvent/List";
        }

        // https://docs.nopcommerce.com/en/developer/plugins/menu-item.html
        public Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            eventMessage.RootMenuItem.InsertAfter("Help",
                new AdminMenuItem
                {
                    SystemName = "ABCWarehouse.EventSurveys",
                    Title = "Event Surveys",
                    IconClass = "far fa-list-alt",
                    Visible = true,
                    Url = eventMessage.GetMenuItemUrl("SurveyEvent", "List")
                });

            return Task.CompletedTask;
        }
    }
}
