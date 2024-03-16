using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace demo001;

class Program
{
    private static readonly ActivitySource MyActivitySource = new("MyCompany.MyProduct.MyLibrary");

    static void Main(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("MyCompany.MyProduct.MyLibrary")
            .AddConsoleExporter()
            .Build();

        using (var activity = MyActivitySource.StartActivity("SayHello"))
        {
            activity?.SetTag("foo", 1);
            activity?.SetTag("bar", "Hello, World!");
            activity?.SetTag("baz", new int[] { 1, 2, 3 });
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
    }

    private static void ExperimentLogScope()
    {
        var logger1 = Global.LogFactory.CreateLogger("L1");
        var logger2 = Global.LogFactory.CreateLogger("L2");
        using (logger1.BeginScope("sc1"))
        {
            GenerateLogs(logger2);
            using (Global.Log.BeginScope("sc11"))
            {
                GenerateLogs(logger2);
            }
        }

        using (Global.Log.BeginScope("sc2"))
        {
            GenerateLogs(logger1);
        }
    }

    private static void GenerateLogs(ILogger logger)
    {
        logger.LogDebug("A1");
        logger.LogInformation("A2");
        logger.LogWarning("A3");
        logger.LogError("A4");
    }
}
