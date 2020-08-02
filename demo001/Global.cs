using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using ToolsPack.NLog;

namespace demo001
{
    internal static class Global
    {
        public static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog(LogQuickConfig.SetupFileAndConsole("./logs/demo001.log"));

            LogQuickConfig.UseNewtonsoftJson();
            JsonConvert.DefaultSettings = () => LogQuickConfig.DefaultJsonSerializerSettings;
        });

        public static readonly ILogger Log = LogFactory.CreateLogger("G");
    }
}
