
using Microsoft.AspNetCore.Hosting;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbcCore;
using System;
using System.Configuration;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;

namespace Nop.Plugin.Misc.AbcStatusApi
{
    public class AbcStatusApiSettings : ISettings
    {
        public bool IsDebugModeEnabled { get; set; }
    }
}