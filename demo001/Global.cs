using Elastic.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace demo001;

internal static class Global
{
    public static readonly IConfiguration Config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    public static readonly ILoggerFactory LogFactory = LoggerFactory.Create(loggingBuilder =>
    {
        loggingBuilder.AddConfiguration(Config.GetSection("Logging"));
        loggingBuilder.AddSeq(Config.GetSection("Logging:Seq"));
        loggingBuilder.AddElasticsearch(elasticConf =>
        {
            elasticConf.ShipTo = Config.GetRequiredSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>()!;
        });
    });

    public static readonly ILogger Log = LogFactory.CreateLogger("G");
}
