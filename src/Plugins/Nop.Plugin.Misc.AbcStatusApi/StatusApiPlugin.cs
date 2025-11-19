using Nop.Services.Common;
using Nop.Services.Plugins;
using Nop.Core;

namespace Nop.Plugin.Misc.AbcStatusApi
{
    public class StatusApiPlugin : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;

        public StatusApiPlugin(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return
                $"{_webHelper.GetStoreLocation()}Admin/StatusApi/Configure";
        }
    }
}
