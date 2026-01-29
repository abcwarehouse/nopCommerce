using Nop.Services.Cms;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.AbcMarkdownDate
{
    public class MarkdownDateWidget : BasePlugin, IWidgetPlugin
    {
        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone) => typeof(Nop.Plugin.Widgets.AbcHomeDeliveryStatus.Components.AbcMarkdownDateViewComponent);

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { "productdetails_before_addtocart" });
        }
    }
}
