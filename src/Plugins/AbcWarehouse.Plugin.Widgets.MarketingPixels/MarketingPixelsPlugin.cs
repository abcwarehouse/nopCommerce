using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace AbcWarehouse.Plugin.Widgets.MarketingPixels;

public class MarketingPixelsPlugin : BasePlugin, IWidgetPlugin
{
    public bool HideInWidgetList => false;

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(Components.MarketingPixelsViewComponent);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        // Rendered in the <head>; the view component decides per page whether anything is output.
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HeadHtmlTag
        });
    }
}
