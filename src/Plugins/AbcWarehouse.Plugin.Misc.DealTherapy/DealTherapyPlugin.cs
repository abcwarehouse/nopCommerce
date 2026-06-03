using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Plugins;

namespace AbcWarehouse.Plugin.Misc.DealTherapy
{
    public class DealTherapyPlugin : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;

        public DealTherapyPlugin(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DealTherapy/Submissions";
        }
    }
}
