using Nop.Core;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.UI;
using Nop.Services.Cms;

public class MyHeaderInfoWidget : BasePlugin, IWidgetPlugin
{
    public IList<string> GetWidgetZones()
    {
        return new List<string> { PublicWidgetZones.HeaderLinksAfter };
    }

    public string GetWidgetViewComponentName(string widgetZone)
    {
        return "SearchSpring";
    }

    public override void Install()
    {
        base.Install();
    }

    public override void Uninstall()
    {
        base.Uninstall();
    }
}
