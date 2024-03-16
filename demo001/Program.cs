using Microsoft.Extensions.Logging;

namespace demo001;

class Program
{
    static void Main(string[] args)
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

        Console.ReadLine();
    }

    private static void GenerateLogs(ILogger logger)
    {
        logger.LogDebug("A1");
        logger.LogInformation("A2");
        logger.LogWarning("A3");
        logger.LogError("A4");
    }
}
