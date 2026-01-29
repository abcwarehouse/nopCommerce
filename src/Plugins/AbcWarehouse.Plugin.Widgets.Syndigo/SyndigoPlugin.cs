using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Task = System.Threading.Tasks.Task;
using Nop.Services.Configuration;
using Nop.Plugin.Misc.AbcCore.Infrastructure;

namespace AbcWarehouse.Plugin.Widgets.Syndigo
{
    public class SyndigoPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IWebHelper _webHelper;

        public SyndigoPlugin(
            IWebHelper webHelper
        )
        {
            _webHelper = webHelper;
        }

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Components.WidgetsSyndigoViewComponent);
        }

        public System.Threading.Tasks.Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                CustomPublicWidgetZones.ProductDetailsDescriptionTabTop
            });
        }
    }
}
